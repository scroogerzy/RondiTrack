import 'package:dio/dio.dart';
import 'package:cookie_jar/cookie_jar.dart';
import 'package:dio_cookie_manager/dio_cookie_manager.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../config/app_config.dart';
import 'access_token_store.dart';
import 'safe_api_log_interceptor.dart';

final accessTokenStoreProvider = Provider<AccessTokenStore>(
  (ref) => AccessTokenStore(),
);

final _refreshCookieJar = CookieJar();

final apiDioProvider = Provider<Dio>((ref) {
  final config = ref.watch(appConfigProvider);
  final tokenStore = ref.watch(accessTokenStoreProvider);
  final refreshDio = Dio(BaseOptions(baseUrl: config.apiBaseUrl.toString()));

  final dio = Dio(
    BaseOptions(
      baseUrl: config.apiBaseUrl.toString(),
      connectTimeout: const Duration(seconds: 10),
      receiveTimeout: const Duration(seconds: 20),
      sendTimeout: const Duration(seconds: 20),
      headers: const {
        'Accept': 'application/json',
        'Content-Type': 'application/json',
      },
    ),
  );
  dio.interceptors.add(CookieManager(_refreshCookieJar));
  refreshDio.interceptors.add(CookieManager(_refreshCookieJar));

  dio.interceptors.add(
    QueuedInterceptorsWrapper(
      onRequest: (options, handler) {
        final token = tokenStore.token;
        if (token != null && token.isNotEmpty) {
          options.headers['Authorization'] = 'Bearer $token';
        }
        handler.next(options);
      },
      onError: (error, handler) async {
        final request = error.requestOptions;
        final isAuthRoute = request.path.contains('/auth/login') ||
            request.path.contains('/auth/refresh') ||
            request.path.contains('/auth/logout');

        if (error.response?.statusCode != 401 ||
            isAuthRoute ||
            request.extra['retriedAfterRefresh'] == true) {
          handler.next(error);
          return;
        }

        try {
          final refreshed = await refreshDio.post<Map<String, dynamic>>(
            '/api/v1/auth/refresh',
          );
          final newToken = refreshed.data?['accessToken']?.toString();
          if (newToken == null || newToken.isEmpty) {
            throw const FormatException('Refresh returned no access token.');
          }

          tokenStore.set(newToken);
          request
            ..headers['Authorization'] = 'Bearer $newToken'
            ..extra['retriedAfterRefresh'] = true;

          handler.resolve(await dio.fetch<dynamic>(request));
        } catch (_) {
          tokenStore.clear();
          handler.next(error);
        }
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

  Future<Map<String, dynamic>> login(String email, String password) =>
      _map('POST', '/api/v1/auth/login',
          data: {'email': email, 'password': password});

  Future<Map<String, dynamic>> register(
    String fullName,
    String email,
    String password,
  ) =>
      _map('POST', '/api/v1/auth/register',
          data: {
            'fullName': fullName,
            'email': email,
            'password': password,
          });

  Future<Map<String, dynamic>> refresh() =>
      _map('POST', '/api/v1/auth/refresh');

  Future<void> logout() async {
    await dio.post<void>('/api/v1/auth/logout');
  }

  Future<Map<String, dynamic>> home() =>
      _map('GET', '/api/v1/me/home');

  Future<Map<String, dynamic>> activity({
    int pageSize = 20,
    String? pageToken,
  }) =>
      _map('GET', '/api/v1/activity', query: {
        'pageSize': pageSize,
        if (pageToken != null) 'pageToken': pageToken,
      });

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
  }) =>
      _map('GET', '/api/v1/stokvels/$stokvelId/cycles', query: {
        'pageSize': pageSize,
        'sortDirection': sortDirection,
        if (pageToken != null) 'pageToken': pageToken,
      });

  Future<Map<String, dynamic>> cycle(
    String stokvelId,
    String cycleId,
  ) =>
      _map('GET', '/api/v1/stokvels/$stokvelId/cycles/$cycleId');

  Future<Map<String, dynamic>> contributions(
    String stokvelId,
    String cycleId, {
    int pageSize = 20,
    String? pageToken,
    String? userId,
    String sortDirection = 'desc',
  }) =>
      _map(
        'GET',
        '/api/v1/stokvels/$stokvelId/cycles/$cycleId/contributions',
        query: {
          'pageSize': pageSize,
          'sortDirection': sortDirection,
          if (pageToken != null) 'pageToken': pageToken,
          if (userId != null) 'userId': userId,
        },
      );

  Future<Map<String, dynamic>> payoutOrder(
    String stokvelId,
    String cycleId,
  ) =>
      _map(
        'GET',
        '/api/v1/stokvels/$stokvelId/cycles/$cycleId/payout-order',
      );

  Future<Map<String, dynamic>> recordContribution({
    required String stokvelId,
    required String cycleId,
    required String userId,
    required int amountInCents,
    required String idempotencyKey,
  }) =>
      _map(
        'POST',
        '/api/v1/stokvels/$stokvelId/cycles/$cycleId/contributions',
        data: {
          'userId': userId,
          'amountInCents': amountInCents,
        },
        options: Options(headers: {'Idempotency-Key': idempotencyKey}),
      );

  Future<void> recordPayout({
    required String stokvelId,
    required String cycleId,
  }) async {
    await dio.post<void>(
      '/api/v1/stokvels/$stokvelId/cycles/$cycleId/payout',
    );
  }
}

final rondiApiProvider = Provider<RondiApi>(
  (ref) => RondiApi(ref.watch(apiDioProvider)),
);




