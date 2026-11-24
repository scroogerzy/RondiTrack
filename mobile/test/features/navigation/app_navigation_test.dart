// Keep widget-test setup explicit so each step remains easy to follow.
// ignore_for_file: cascade_invocations
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import '../../support/fake_auth_session.dart';
import 'package:ronditrack_mobile/features/auth/auth_session.dart';
import 'package:ronditrack_mobile/app/app.dart';
import 'package:ronditrack_mobile/core/config/app_config.dart';
import 'package:ronditrack_mobile/features/home/data/home_repository.dart';
import 'package:ronditrack_mobile/features/navigation/app_router.dart';

void main() {
  testWidgets('signed out app is gated by sign in', (tester) async {
    await tester.pumpWidget(
      ProviderScope(
        retry: (count, error) => null,
        overrides: [
        authSessionProvider.overrideWith(FakeAuthSessionController.new),
          appConfigProvider.overrideWithValue(
            AppConfig('dev', Uri.parse('http://localhost')),
          ),
          homeRepositoryProvider.overrideWith(
            (ref) => FakeHomeRepository(delay: Duration.zero),
          ),
        ],
        child: const RondiTrackApp(),
      ),
    );
    await tester.pumpAndSettle();
    await signInWithDemoCredentials(tester);
    expect(find.text('Your stokvels'), findsOneWidget);
    await tester.tap(find.text('Activity'));
    await tester.pumpAndSettle();
    expect(find.text('Activity'), findsWidgets);
    await tester.tap(find.text('Stokvels'));
    await tester.pumpAndSettle();
    expect(find.text('Ubuntu Savings Club'), findsOneWidget);
  });

  testWidgets('invite opened signed out survives fake sign in', (tester) async {
    final container = ProviderContainer(
      overrides: [
        authSessionProvider.overrideWith(FakeAuthSessionController.new),
        appConfigProvider.overrideWithValue(
          AppConfig('dev', Uri.parse('http://localhost')),
        ),
      ],
    );
    final router = container.read(appRouterProvider);
    router.go('/join/INVITE-42');
    await tester.pumpWidget(
      UncontrolledProviderScope(
        container: container,
        child: const RondiTrackApp(),
      ),
    );
    await tester.pumpAndSettle();
    expect(find.text('Sign in to RondiTrack'), findsOneWidget);
    await signInWithDemoCredentials(tester);
    expect(find.text('You are invited to join a stokvel'), findsOneWidget);
    container.dispose();
  });

  testWidgets('profile sign out returns to sign in', (tester) async {
    await tester.pumpWidget(
      ProviderScope(
        retry: (count, error) => null,
        overrides: [
        authSessionProvider.overrideWith(FakeAuthSessionController.new),
          appConfigProvider.overrideWithValue(
            AppConfig('dev', Uri.parse('http://localhost')),
          ),
          homeRepositoryProvider.overrideWith(
            (ref) => FakeHomeRepository(delay: Duration.zero),
          ),
        ],
        child: const RondiTrackApp(),
      ),
    );
    await tester.pumpAndSettle();
    await signInWithDemoCredentials(tester);
    await tester.tap(find.text('Stokvels'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Ubuntu Savings Club'));
    await tester.pumpAndSettle();
    expect(find.text('Confirm contributions'), findsOneWidget);
    await tester.pageBack();
    await tester.pumpAndSettle();
    await tester.tap(find.text('Mzanzi Monthly Stokvel'));
    await tester.pumpAndSettle();
    expect(find.text('Confirm contributions'), findsNothing);
  });

  testWidgets('stokvel branch retains its nested cycle when switching tabs', (
    tester,
  ) async {
    await tester.pumpWidget(
      ProviderScope(
        retry: (count, error) => null,
        overrides: [
        authSessionProvider.overrideWith(FakeAuthSessionController.new),
          appConfigProvider.overrideWithValue(
            AppConfig('dev', Uri.parse('http://localhost')),
          ),
          homeRepositoryProvider.overrideWith(
            (ref) => FakeHomeRepository(delay: Duration.zero),
          ),
        ],
        child: const RondiTrackApp(),
      ),
    );
    await tester.pumpAndSettle();
    await signInWithDemoCredentials(tester);
    await tester.tap(find.text('Stokvels'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Ubuntu Savings Club'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('View cycle'));
    await tester.pumpAndSettle();
    expect(find.text('Cycle 5'), findsOneWidget);
    await tester.tap(find.text('Activity'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Stokvels'));
    await tester.pumpAndSettle();
    expect(find.text('Contributions'), findsOneWidget);
  });

  testWidgets('a cycle from a non-member stokvel is also not found', (
    tester,
  ) async {
    final container = ProviderContainer(
      overrides: [
        authSessionProvider.overrideWith(FakeAuthSessionController.new),
        appConfigProvider.overrideWithValue(
          AppConfig('dev', Uri.parse('http://localhost')),
        ),
        homeRepositoryProvider.overrideWith(
          (ref) => FakeHomeRepository(
            persona: DemoPersona.sibusiso,
            delay: Duration.zero,
          ),
        ),
      ],
    );
    final router = container.read(appRouterProvider);
    await tester.pumpWidget(
      UncontrolledProviderScope(
        container: container,
        child: const RondiTrackApp(),
      ),
    );
    await tester.pumpAndSettle();
    await signInWithDemoCredentials(tester);
    router.go('/stokvels/mzanzi-monthly-stokvel/cycles/mzanzi-cycle-5');
    await tester.pumpAndSettle();
    expect(find.text('Cycle not found.'), findsOneWidget);
    container.dispose();
  });
}
