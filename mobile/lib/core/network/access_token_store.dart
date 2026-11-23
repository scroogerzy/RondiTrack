class AccessTokenStore {
  String? _token;

  String? get token => _token;

  void set(String? value) {
    _token = value;
  }

  void clear() {
    _token = null;
  }
}
