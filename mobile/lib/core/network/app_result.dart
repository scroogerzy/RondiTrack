import 'failure.dart';

sealed class AppResult<T> {
  const AppResult();

  factory AppResult.success(T value) = Success<T>;
  factory AppResult.failure(Failure failure) = Failed<T>;
}

final class Success<T> extends AppResult<T> {
  const Success(this.value);
  final T value;
}

final class Failed<T> extends AppResult<T> {
  const Failed(this.failure);
  final Failure failure;
}
