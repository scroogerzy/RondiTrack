import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ronditrack_mobile/app/app.dart';
import '../../support/fake_auth_session.dart';
import 'package:ronditrack_mobile/features/auth/auth_session.dart';
import 'package:ronditrack_mobile/core/config/app_config.dart';
import 'package:ronditrack_mobile/features/home/data/home_repository.dart';
import 'package:ronditrack_mobile/features/navigation/app_router.dart';

void main() {
  testWidgets('compact 360 dp layout remains scrollable at 200 percent text', (
    tester,
  ) async {
    tester.view.physicalSize = const Size(360, 640);
    tester.view.devicePixelRatio = 1;
    tester.platformDispatcher.textScaleFactorTestValue = 2;
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);
    addTearDown(tester.platformDispatcher.clearTextScaleFactorTestValue);

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
    final container = ProviderScope.containerOf(
      tester.element(find.byType(RondiTrackApp)),
    );
    await signInWithDemoCredentials(tester);
    await tester.pumpAndSettle();
    expect(tester.takeException(), isNull);
    await tester.tap(find.text('Stokvels'));
    await tester.pumpAndSettle();
    expect(tester.takeException(), isNull);
    expect(find.text('Ubuntu Savings Club'), findsOneWidget);
    await tester.tap(find.text('Ubuntu Savings Club'));
    await tester.pumpAndSettle();
    expect(find.text('Current cycle'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });

  testWidgets('expanded layout places list and detail on one address', (
    tester,
  ) async {
    tester.view.physicalSize = const Size(1100, 800);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);

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
    final container = ProviderScope.containerOf(
      tester.element(find.byType(RondiTrackApp)),
    );
    container.read(appRouterProvider).go('/stokvels/ubuntu-savings-club');
    await tester.pumpAndSettle();
    expect(find.text('Ubuntu Savings Club'), findsWidgets);
    expect(find.text('Current cycle'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });
}
