import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class AccessTokenStore {
  static const _accessKey = 'ronditrack.access_token';
  static const _refreshKey = 'ronditrack.refresh_cookie';

  static const FlutterSecureStorage _storage = FlutterSecureStorage();

  String? _token;
  String? _refreshCookie;
  Future<void>? _loading;

  String? get token => _token;

  Future<void> load() {
    return _loading ??= _load();
  }

  Future<void> _load() async {
    _token = await _storage.read(key: _accessKey);
    _refreshCookie = await _storage.read(key: _refreshKey);
  }

  Future<void> set(String? value) async {
    _token = value;
    if (value == null || value.isEmpty) {
      await _storage.delete(key: _accessKey);
    } else {
      await _storage.write(key: _accessKey, value: value);
    }
  }

  Future<void> saveRefreshCookie(String? value) async {
    if (value == null || value.isEmpty) return;
    _refreshCookie = value;
    await _storage.write(key: _refreshKey, value: value);
  }

  String? get refreshCookie => _refreshCookie;

  Future<void> clear() async {
    _token = null;
    _refreshCookie = null;
    await _storage.delete(key: _accessKey);
    await _storage.delete(key: _refreshKey);
  }
}
