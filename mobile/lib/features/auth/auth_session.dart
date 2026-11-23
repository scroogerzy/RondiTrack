import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/network/access_token_store.dart';
import '../../core/network/failure.dart';
import '../../core/network/rondi_api.dart';

class AuthSession {
  const AuthSession({
    required this.userId,
    required this.email,
    required this.expiresAt,
  });

  final String userId;
  final String email;
  final DateTime expiresAt;
}

final authSessionProvider =
    NotifierProvider<AuthSessionController, AuthSession?>(
  AuthSessionController.new,
);

class AuthSessionController extends Notifier<AuthSession?> {
  late RondiApi _api;
  late AccessTokenStore _tokens;

  @override
  AuthSession? build() {
    _api = ref.watch(rondiApiProvider);
    _tokens = ref.watch(accessTokenStoreProvider);
    return null;
  }

  Future<void> signIn(String email, String password) async {
    try {
      final response = await _api.login(email, password);
      final token = response['accessToken']?.toString();
      if (token == null || token.isEmpty) {
        throw const FormatException('The API did not return an access token.');
      }

      _tokens.set(token);
      final home = await _api.home();
      final expires = DateTime.parse(response['expiresAt'].toString()).toUtc();

      state = AuthSession(
        userId: home['currentUserId'].toString(),
        email: email,
        expiresAt: expires,
      );
    } catch (error) {
      _tokens.clear();
      throw mapFailure(error);
    }
  }

  Future<void> signOut() async {
    try {
      await _api.logout();
    } catch (_) {
      // Local session is cleared even if the server cannot be reached.
    } finally {
      _tokens.clear();
      state = null;
    }
  }
}
