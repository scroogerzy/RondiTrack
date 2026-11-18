// Keep widget-test setup explicit so each step remains easy to follow.
// ignore_for_file: cascade_invocations
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ronditrack_mobile/app/app.dart';
import 'package:ronditrack_mobile/features/home/data/home_repository.dart';
import 'package:ronditrack_mobile/features/navigation/app_router.dart';

void main() {
  testWidgets('signed out app is gated by sign in', (tester) async {
    await tester.pumpWidget(
      ProviderScope(
        retry: (count, error) => null,
        child: const RondiTrackApp(),
      ),
    );
    await tester.pumpAndSettle();
    expect(find.text('Sign in to RondiTrack'), findsOneWidget);
  });

  testWidgets('sign in lands on Home and tabs remain available', (
    tester,
  ) async {
    await tester.pumpWidget(
      ProviderScope(
        retry: (count, error) => null,
        overrides: [
          homeRepositoryProvider.overrideWith(
            (ref) => FakeHomeRepository(delay: Duration.zero),
          ),
        ],
        child: const RondiTrackApp(),
      ),
    );
    await tester.pumpAndSettle();
    await tester.tap(find.text('Sign in'));
    await tester.pumpAndSettle();
    expect(find.text('Your stokvels'), findsOneWidget);
    await tester.tap(find.text('Activity'));
    await tester.pumpAndSettle();
    expect(find.text('Activity'), findsWidgets);
    await tester.tap(find.text('Stokvels'));
    await tester.pumpAndSettle();
    expect(find.text('Ubuntu Savings Club'), findsOneWidget);
  });

  testWidgets('invite opened signed out survives fake sign in', (tester) async {
    final container = ProviderContainer();
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
    await tester.tap(find.text('Sign in'));
    await tester.pumpAndSettle();
    expect(find.text('You are invited to join a stokvel'), findsOneWidget);
    container.dispose();
  });

  testWidgets('profile sign out returns to sign in', (tester) async {
    await tester.pumpWidget(const ProviderScope(child: RondiTrackApp()));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Sign in'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Profile'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Sign out'));
    await tester.pumpAndSettle();
    expect(find.text('Sign in to RondiTrack'), findsOneWidget);
    await tester.binding.handlePopRoute();
    await tester.pumpAndSettle();
    expect(find.text('Sign in to RondiTrack'), findsOneWidget);
  });

  testWidgets('a non-member sees not found for a stokvel deep link', (
    tester,
  ) async {
    final container = ProviderContainer(
      overrides: [
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
    await tester.tap(find.text('Sign in'));
    await tester.pumpAndSettle();
    router.go('/stokvels/mzanzi-monthly-stokvel');
    await tester.pumpAndSettle();
    expect(find.text('Stokvel not found.'), findsOneWidget);
    container.dispose();
  });

  testWidgets('Lerato sees treasurer actions only in her treasurer stokvel', (
    tester,
  ) async {
    await tester.pumpWidget(
      ProviderScope(
        retry: (count, error) => null,
        overrides: [
          homeRepositoryProvider.overrideWith(
            (ref) => FakeHomeRepository(delay: Duration.zero),
          ),
        ],
        child: const RondiTrackApp(),
      ),
    );
    await tester.pumpAndSettle();
    await tester.tap(find.text('Sign in'));
    await tester.pumpAndSettle();
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
          homeRepositoryProvider.overrideWith(
            (ref) => FakeHomeRepository(delay: Duration.zero),
          ),
        ],
        child: const RondiTrackApp(),
      ),
    );
    await tester.pumpAndSettle();
    await tester.tap(find.text('Sign in'));
    await tester.pumpAndSettle();
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
    await tester.tap(find.text('Sign in'));
    await tester.pumpAndSettle();
    router.go('/stokvels/mzanzi-monthly-stokvel/cycles/mzanzi-cycle-5');
    await tester.pumpAndSettle();
    expect(find.text('Cycle not found.'), findsOneWidget);
    container.dispose();
  });
}
