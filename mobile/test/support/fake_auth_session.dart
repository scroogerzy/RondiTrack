import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ronditrack_mobile/features/auth/auth_session.dart';

class FakeAuthSessionController extends AuthSessionController {
  @override
  AuthSession? build() => null;

  @override
  Future<void> signIn(String email, String password) async {
    state = AuthSession(
      userId: 'demo-user',
      email: email,
      expiresAt: DateTime.utc(2030),
    );
  }

  @override
  Future<void> signOut() async {
    state = null;
  }
}

Future<void> signInWithDemoCredentials(WidgetTester tester) async {
  final fields = find.byType(TextFormField);

  await tester.enterText(fields.at(0), 'lerato@example.com');
  await tester.enterText(fields.at(1), 'DemoPassword123!');
  await tester.tap(find.text('Sign in'));
  await tester.pumpAndSettle();
}
