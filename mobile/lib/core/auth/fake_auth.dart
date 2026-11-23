import 'package:flutter_riverpod/flutter_riverpod.dart';

/// The three capstone demo personas available to fake sign-in.
class FakeAuthState {
  const new signedOut() : email = null;
  const new signedIn(this.email);

  final String? email;
  bool get isSignedIn => email != null;
  String get displayName => switch (email) {
    'lerato@example.com' => 'Lerato Dlamini',
    'sipho@example.com' => 'Sipho Ndlovu',
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
      'sipho@example.com',
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


