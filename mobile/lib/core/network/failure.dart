import 'package:dio/dio.dart';

sealed class Failure {
  const Failure();
  String get message;
  factory Failure.from(Object error) => mapFailure(error);
}

final class ValidationFailure extends Failure {
  const ValidationFailure(this.fields);
  final Map<String, List<String>> fields;
  @override
  String get message => fields.values.expand((e) => e).join('\n');
}

final class UnauthenticatedFailure extends Failure {
  const UnauthenticatedFailure();
  @override
  String get message => 'Please sign in again.';
}

final class ForbiddenFailure extends Failure {
  const ForbiddenFailure([this.detail]);
  final String? detail;
  @override
  String get message => detail ?? 'You do not have permission to do this.';
}

final class NotFoundFailure extends Failure {
  const NotFoundFailure([this.detail]);
  final String? detail;
  @override
  String get message => detail ?? 'The requested information was not found.';
}

final class ConflictFailure extends Failure {
  const ConflictFailure([this.detail]);
  final String? detail;
  @override
  String get message => detail ?? 'This information has changed. Refresh and try again.';
}

final class RuleFailure extends Failure {
  const RuleFailure(this.detail);
  final String detail;
  @override
  String get message => detail;
}

final class RateLimitedFailure extends Failure {
  const RateLimitedFailure(this.waitSeconds);
  final int waitSeconds;
  @override
  String get message => 'Please wait $waitSeconds seconds before trying again.';
}

final class ServerFailure extends Failure {
  const ServerFailure({this.correlationId, this.detail});
  final String? correlationId;
  final String? detail;
  @override
  String get message =>
      '${detail ?? 'Something went wrong.'} Reference: ${correlationId ?? 'unavailable'}';
}

final class NoAnswerFailure extends Failure {
  const NoAnswerFailure();
  @override
  String get message => 'You’re offline, or we couldn’t reach the server.';
}

Failure mapFailure(Object error) {
  if (error is Failure) return error;

  if (error is DioException) {
    final response = error.response;
    if (response == null) return const NoAnswerFailure();

    final status = response.statusCode ?? 0;
    final data = response.data;
    final problem = data is Map ? data : <dynamic, dynamic>{};
    final detail = problem['detail']?.toString();
    final correlationId =
        problem['correlationId']?.toString() ??
        response.headers.value('X-Correlation-ID');

    if (status == 400 || status == 422) {
      final errors = problem['errors'];
      if (errors is Map) {
        return ValidationFailure({
          for (final entry in errors.entries)
            entry.key.toString(): entry.value is List
                ? (entry.value as List).map((e) => e.toString()).toList()
                : [entry.value.toString()],
        });
      }
      return RuleFailure(detail ?? 'Please check the information and try again.');
    }
    if (status == 401) return const UnauthenticatedFailure();
    if (status == 403) return ForbiddenFailure(detail);
    if (status == 404) return NotFoundFailure(detail);
    if (status == 409 || status == 412) return ConflictFailure(detail);
    if (status == 429) {
      final seconds = int.tryParse(
        response.headers.value('Retry-After') ?? '',
      );
      return RateLimitedFailure(seconds != null && seconds > 0 ? seconds : 60);
    }
    if (status >= 500) {
      return ServerFailure(correlationId: correlationId);
    }
    return RuleFailure(detail ?? 'The request could not be completed.');
  }

  if (error is FormatException) {
    return const ServerFailure(detail: 'The server returned an invalid response.');
  }
  return const ServerFailure(detail: 'An unexpected error occurred.');
}
