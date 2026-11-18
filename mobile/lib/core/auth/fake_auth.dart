import 'package:flutter_riverpod/flutter_riverpod.dart';

/// The three API seed personas available to the Week 10 fake sign-in.
class FakeAuthState {
  const new signedOut() : email = null;
  const new signedIn(this.email);

  final String? email;
  bool get isSignedIn => email != null;
  String get displayName => switch (email) {
    'lerato@example.com' => 'Lerato Dlamini',
    'sibusiso@example.com' => 'Sibusiso Ndlovu',
    'thabo@example.com' => 'Thabo Mokoena',
    _ => 'Signed out',
  };
}

class FakeAuthController extends Notifier<FakeAuthState> {
  @override
  FakeAuthState build() => const FakeAuthState.signedOut();

  bool signIn(String email) {
    final normalized = email.trim().toLowerCase();
    const allowed = {
      'lerato@example.com',
      'sibusiso@example.com',
      'thabo@example.com',
    };
    if (!allowed.contains(normalized)) return false;
    state = FakeAuthState.signedIn(normalized);
    return true;
  }

  void signOut() => state = const FakeAuthState.signedOut();
}

final fakeAuthProvider = NotifierProvider<FakeAuthController, FakeAuthState>(
  FakeAuthController.new,
);

/// Pure redirect policy. Only an in-app invite address may survive sign-in.
String? authRedirect({
  required bool isSignedIn,
  required String location,
  String? rememberedLocation,
}) {
  final path = Uri.tryParse(location)?.path ?? location;
  if (!isSignedIn) {
    if (path == '/sign-in') return null;
    final invite = _safeInvitePath(path);
    if (invite != null) {
      return Uri(
        path: '/sign-in',
        queryParameters: {'from': invite},
      ).toString();
    }
    return '/sign-in';
  }

  if (path == '/sign-in' || path == '/') {
    return _safeInvitePath(rememberedLocation) ?? '/home';
  }
  return null;
}

String? _safeInvitePath(String? value) {
  if (value == null || value.isEmpty) return null;
  final uri = Uri.tryParse(value);
  if (uri == null ||
      uri.hasScheme ||
      uri.hasAuthority ||
      !uri.path.startsWith('/join/')) {
    return null;
  }
  if (uri.pathSegments.length != 2 || uri.pathSegments.last.isEmpty) {
    return null;
  }
  return uri.path;
}
