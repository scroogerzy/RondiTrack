import 'package:flutter_test/flutter_test.dart';
import 'package:ronditrack_mobile/core/auth/fake_auth.dart';

void main() {
  test('signed out navigation goes to sign in', () {
    expect(authRedirect(isSignedIn: false, location: '/home'), '/sign-in');
  });
  test('signed out invite is remembered through sign in', () {
    expect(
      authRedirect(isSignedIn: false, location: '/join/ABC-123'),
      '/sign-in?from=%2Fjoin%2FABC-123',
    );
  });
  test('signed in returns to remembered invite', () {
    expect(
      authRedirect(
        isSignedIn: true,
        location: '/sign-in',
        rememberedLocation: '/join/ABC-123',
      ),
      '/join/ABC-123',
    );
  });
  test('external address is never remembered', () {
    expect(
      authRedirect(
        isSignedIn: true,
        location: '/sign-in',
        rememberedLocation: 'https://evil.example/join/x',
      ),
      '/home',
    );
    expect(
      authRedirect(
        isSignedIn: false,
        location: '/join/ABC',
        rememberedLocation: '//evil.example',
      ),
      '/sign-in?from=%2Fjoin%2FABC',
    );
  });
  test('signed in sign-in route returns home when no invite exists', () {
    expect(authRedirect(isSignedIn: true, location: '/sign-in'), '/home');
  });
  test('signed in ordinary navigation is not redirected', () {
    expect(
      authRedirect(isSignedIn: true, location: '/stokvels/ubuntu-savings-club'),
      isNull,
    );
  });
}
