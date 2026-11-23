import 'package:dio/dio.dart';

class SafeApiLogInterceptor extends Interceptor {
  SafeApiLogInterceptor({void Function(String message)? write})
      : _write = write ?? print;

  final void Function(String message) _write;

  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) {
    options.extra['requestStartedAt'] = DateTime.now();
    _write('API ${options.method} ${options.path}');
    handler.next(options);
  }

  @override
  void onResponse(Response response, ResponseInterceptorHandler handler) {
    final started = response.requestOptions.extra['requestStartedAt'];
    final elapsed = started is DateTime
        ? DateTime.now().difference(started).inMilliseconds
        : -1;
    final correlationId =
        response.headers.value('X-Correlation-ID') ?? 'none';
    _write(
      'API ${response.requestOptions.method} ${response.requestOptions.path} '
      '${response.statusCode} ${elapsed}ms correlationId=$correlationId',
    );
    handler.next(response);
  }

  @override
  void onError(DioException error, ErrorInterceptorHandler handler) {
    final response = error.response;
    final correlationId =
        response?.headers.value('X-Correlation-ID') ?? 'none';
    _write(
      'API ${error.requestOptions.method} ${error.requestOptions.path} '
      '${response?.statusCode ?? 'no-response'} correlationId=$correlationId',
    );
    handler.next(error);
  }
}
