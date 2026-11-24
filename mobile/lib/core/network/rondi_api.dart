import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../config/app_config.dart';
import 'access_token_store.dart';
import 'safe_api_log_interceptor.dart';

final accessTokenStoreProvider = Provider<AccessTokenStore>(
  (ref) => AccessTokenStore(),
);

final apiDioProvider = Provider<Dio>((ref) {
  final config = ref.watch(appConfigProvider);
  final tokenStore = ref.watch(accessTokenStoreProvider);

  final options = BaseOptions(
    baseUrl: config.apiBaseUrl.toString(),
    connectTimeout: const Duration(seconds: 10),
    receiveTimeout: const Duration(seconds: 20),
    sendTimeout: const Duration(seconds: 20),
    headers: const {
      'Accept': 'application/json',
      'Content-Type': 'application/json',
    },
  );

  final dio = Dio(options);
  final refreshDio = Dio(options);
  Future<String>? refreshInFlight;

  Future<String> refreshAccessToken() {
    final existing = refreshInFlight;
    if (existing != null) return existing;

    final future = refreshDio
        .post<Map<String, dynamic>>('/api/v1/auth/refresh')
        .then((response) async {
          final token = response.data?['accessToken']?.toString();
          if (token == null || token.isEmpty) {
            throw const FormatException('Refresh returned no access token.');
          }
          await tokenStore.set(token);
          return token;
        });

    refreshInFlight = future;
    return future.whenComplete(() => refreshInFlight = null);
  }

  void captureRefreshCookie(Response<dynamic> response) {
    final values = response.headers.map['set-cookie'] ?? const <String>[];
    for (final header in values) {
      final match = RegExp(r'(?i)refresh_token=([^;]+)').firstMatch(header);
      if (match != null) {
        tokenStore.saveRefreshCookie('refresh_token=${match.group(1)}');
      }
    }
  }

  void attachRefreshCookie(RequestOptions options) {
    final cookie = tokenStore.refreshCookie;
    if (cookie != null &&
        (options.path.contains('/auth/refresh') ||
            options.path.contains('/auth/logout'))) {
      options.headers['Cookie'] = cookie;
    }
  }

  dio.interceptors.add(
    InterceptorsWrapper(
      onRequest: (options, handler) async {
        try {
          await tokenStore.load();
          final token = tokenStore.token;
          if (token != null && token.isNotEmpty) {
            options.headers['Authorization'] = 'Bearer $token';
          }
          attachRefreshCookie(options);
          handler.next(options);
        } catch (error) {
          handler.reject(DioException(requestOptions: options, error: error));
        }
      },
      onResponse: (response, handler) {
        captureRefreshCookie(response);
        handler.next(response);
      },
      onError: (error, handler) async {
        final request = error.requestOptions;
        final path = request.path;
        final isAuthRoute =
            path.contains('/auth/login') ||
            path.contains('/auth/register') ||
            path.contains('/auth/refresh') ||
            path.contains('/auth/logout');

        if (error.response?.statusCode != 401 ||
            isAuthRoute ||
            request.extra['retriedAfterRefresh'] == true) {
          handler.next(error);
          return;
        }

        try {
          final newToken = await refreshAccessToken();
          request
            ..headers['Authorization'] = 'Bearer $newToken'
            ..extra['retriedAfterRefresh'] = true;

          handler.resolve(await dio.fetch<dynamic>(request));
        } catch (_) {
          await tokenStore.clear();
          handler.next(error);
        }
      },
    ),
  );

  refreshDio.interceptors.add(
    InterceptorsWrapper(
      onRequest: (options, handler) async {
        await tokenStore.load();
        final cookie = tokenStore.refreshCookie;
        if (cookie != null) options.headers['Cookie'] = cookie;
        handler.next(options);
      },
      onResponse: (response, handler) {
        captureRefreshCookie(response);
        handler.next(response);
      },
    ),
  );

  dio.interceptors.add(SafeApiLogInterceptor());

  ref.onDispose(dio.close);
  ref.onDispose(refreshDio.close);
  return dio;
});

class RondiApi {
  RondiApi(this.dio);

  final Dio dio;

  Future<Map<String, dynamic>> _map(
    String method,
    String path, {
    Map<String, dynamic>? data,
    Map<String, dynamic>? query,
    Options? options,
  }) async {
    final response = await dio.request<dynamic>(
      path,
      data: data,
      queryParameters: query,
      options: (options ?? Options()).copyWith(method: method),
    );
    final body = response.data;
    if (body is Map<String, dynamic>) return body;
    if (body is Map) return Map<String, dynamic>.from(body);
    throw const FormatException('Unexpected API response.');
  }

  Future<Map<String, dynamic>> login(String email, String password) => _map(
    'POST',
    '/api/v1/auth/login',
    data: {'email': email, 'password': password},
  );

  Future<Map<String, dynamic>> register(
    String fullName,
    String email,
    String password,
  ) => _map(
    'POST',
    '/api/v1/auth/register',
    data: {'fullName': fullName, 'email': email, 'password': password},
  );

  Future<Map<String, dynamic>> refresh() =>
      _map('POST', '/api/v1/auth/refresh');

  Future<void> logout() async {
    await dio.post<void>('/api/v1/auth/logout');
  }

  Future<Map<String, dynamic>> home() => _map('GET', '/api/v1/me/home');

  Future<Map<String, dynamic>> activity({
    int pageSize = 20,
    String? pageToken,
  }) => _map(
    'GET',
    '/api/v1/activity',
    query: {
      'pageSize': pageSize,
      if (pageToken != null) 'pageToken': pageToken,
    },
  );

  Future<Map<String, dynamic>> stokvel(String id) =>
      _map('GET', '/api/v1/stokvels/$id');

  Future<List<dynamic>> members(String stokvelId) async {
    final response = await dio.get<dynamic>(
      '/api/v1/stokvels/$stokvelId/members',
    );
    return response.data is List ? response.data as List : [];
  }

  Future<Map<String, dynamic>> cycles(
    String stokvelId, {
    int pageSize = 20,
    String? pageToken,
    String sortDirection = 'desc',
  }) => _map(
    'GET',
    '/api/v1/stokvels/$stokvelId/cycles',
    query: {
      'pageSize': pageSize,
      'sortDirection': sortDirection,
      if (pageToken != null) 'pageToken': pageToken,
    },
  );

  Future<Map<String, dynamic>> cycle(String stokvelId, String cycleId) =>
      _map('GET', '/api/v1/stokvels/$stokvelId/cycles/$cycleId');

  Future<Map<String, dynamic>> contributions(
    String stokvelId,
    String cycleId, {
    int pageSize = 20,
    String? pageToken,
    String? userId,
    String sortDirection = 'desc',
  }) => _map(
    'GET',
    '/api/v1/stokvels/$stokvelId/cycles/$cycleId/contributions',
    query: {
      'pageSize': pageSize,
      'sortDirection': sortDirection,
      if (pageToken != null) 'pageToken': pageToken,
      if (userId != null) 'userId': userId,
    },
  );

  Future<Map<String, dynamic>> payoutOrder(String stokvelId, String cycleId) =>
      _map('GET', '/api/v1/stokvels/$stokvelId/cycles/$cycleId/payout-order');

  Future<Map<String, dynamic>> recordContribution({
    required String stokvelId,
    required String cycleId,
    required String userId,
    required int amountInCents,
    required String idempotencyKey,
  }) => _map(
    'POST',
    '/api/v1/stokvels/$stokvelId/cycles/$cycleId/contributions',
    data: {'userId': userId, 'amountInCents': amountInCents},
    options: Options(headers: {'Idempotency-Key': idempotencyKey}),
  );

  Future<void> recordPayout({
    required String stokvelId,
    required String cycleId,
  }) async {
    await dio.post<void>('/api/v1/stokvels/$stokvelId/cycles/$cycleId/payout');
  }
}

final rondiApiProvider = Provider<RondiApi>(
  (ref) => RondiApi(ref.watch(apiDioProvider)),
);
