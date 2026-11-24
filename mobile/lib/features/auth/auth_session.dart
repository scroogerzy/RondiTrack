import 'dart:async';

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
  bool _restoring = false;

  @override
  AuthSession? build() {
    _api = ref.watch(rondiApiProvider);
    _tokens = ref.watch(accessTokenStoreProvider);
    unawaited(_restoreSession());
    return null;
  }

  Future<void> _restoreSession() async {
    if (_restoring) return;
    _restoring = true;

    try {
      await _tokens.load();
      if (_tokens.refreshCookie == null) {
        await _tokens.clear();
        return;
      }

      final response = await _api.refresh();
      final token = response['accessToken']?.toString();
      if (token == null || token.isEmpty) {
        throw const FormatException('Refresh returned no access token.');
      }

      await _tokens.set(token);
      final home = await _api.home();
      final userId = home['currentUserId']?.toString();
      if (userId == null || userId.isEmpty) {
        throw const FormatException('The account response is incomplete.');
      }

      final expires = DateTime.tryParse(response['expiresAt']?.toString() ?? '')
          ?.toUtc();

      state = AuthSession(
        userId: userId,
        email: '',
        expiresAt:
            expires ?? DateTime.now().toUtc().add(const Duration(minutes: 15)),
      );
    } catch (_) {
      await _tokens.clear();
      state = null;
    } finally {
      _restoring = false;
    }
  }

  Future<void> signIn(String email, String password) async {
    try {
      final response = await _api.login(email.trim(), password);
      final token = response['accessToken']?.toString();
      if (token == null || token.isEmpty) {
        throw const FormatException('The API did not return an access token.');
      }

      await _tokens.set(token);
      final home = await _api.home();
      final userId = home['currentUserId']?.toString();
      if (userId == null || userId.isEmpty) {
        throw const FormatException('The account response is incomplete.');
      }

      final expires = DateTime.tryParse(response['expiresAt']?.toString() ?? '')
          ?.toUtc();

      state = AuthSession(
        userId: userId,
        email: email.trim(),
        expiresAt:
            expires ?? DateTime.now().toUtc().add(const Duration(minutes: 15)),
      );
    } catch (error) {
      await _tokens.clear();
      state = null;
      throw mapFailure(error);
    }
  }

  Future<void> signOut() async {
    try {
      await _tokens.load();
      await _api.logout();
    } catch (_) {
      // Always clear local credentials, even when offline.
    } finally {
      await _tokens.clear();
      state = null;
    }
  }
}
