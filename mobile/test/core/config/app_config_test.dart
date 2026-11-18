import 'package:flutter_test/flutter_test.dart';
import 'package:ronditrack_mobile/core/config/app_config.dart';

void main() {
  test('accepts matching native flavor and config', () {
    final config = AppConfig.fromValues(
      nativeFlavor: 'dev',
      environment: 'dev',
      rawUrl: 'https://dev.example.test',
    );
    expect(config.environment, 'dev');
    expect(config.apiBaseUrl.host, 'dev.example.test');
  });
  test('refuses missing values', () {
    expect(
      () => AppConfig.fromValues(
        nativeFlavor: 'dev',
        environment: '',
        rawUrl: '',
      ),
      throwsStateError,
    );
  });
  test('refuses a native flavor/config mismatch', () {
    expect(
      () => AppConfig.fromValues(
        nativeFlavor: 'prod',
        environment: 'dev',
        rawUrl: 'https://dev.example.test',
      ),
      throwsStateError,
    );
  });
  test('production requires HTTPS', () {
    expect(
      () => AppConfig.fromValues(
        nativeFlavor: 'prod',
        environment: 'prod',
        rawUrl: 'http://api.example.test',
      ),
      throwsStateError,
    );
  });
}
