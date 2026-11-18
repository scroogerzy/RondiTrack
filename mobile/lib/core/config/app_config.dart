class AppConfig {
  const new(this.environment, this.apiBaseUrl);

  factory load({required String nativeFlavor}) {
    const environment = String.fromEnvironment('APP_ENV');
    const rawUrl = String.fromEnvironment('API_BASE_URL');
    return AppConfig.fromValues(
      nativeFlavor: nativeFlavor,
      environment: environment,
      rawUrl: rawUrl,
    );
  }

  factory fromValues({
    required String nativeFlavor,
    required String environment,
    required String rawUrl,
  }) {
    if (environment.isEmpty || rawUrl.isEmpty) {
      throw StateError(
        'Missing APP_ENV or API_BASE_URL; pass --dart-define-from-file.',
      );
    }
    if (nativeFlavor.isEmpty || environment != nativeFlavor) {
      throw StateError(
        'Native flavor/config mismatch: $nativeFlavor vs $environment.',
      );
    }
    final uri = Uri.tryParse(rawUrl);
    if (uri == null ||
        !uri.hasScheme ||
        uri.host.isEmpty ||
        !{'http', 'https'}.contains(uri.scheme)) {
      throw const FormatException('Invalid API URL.');
    }
    if (environment == 'prod' && uri.scheme != 'https') {
      throw StateError('Production requires HTTPS.');
    }
    return AppConfig(environment, uri);
  }

  final String environment;
  final Uri apiBaseUrl;
}
