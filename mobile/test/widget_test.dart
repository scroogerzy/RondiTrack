import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ronditrack_mobile/app/app.dart';
import 'package:ronditrack_mobile/core/config/app_config.dart';
import 'package:ronditrack_mobile/features/auth/auth_session.dart';
import 'support/fake_auth_session.dart';

void main() {
  testWidgets('RondiTrack app gates signed-out users', (tester) async {
    await tester.pumpWidget(
      ProviderScope(
        retry: (retryCount, error) => null,
        overrides: [
          appConfigProvider.overrideWithValue(
            AppConfig('dev', Uri.parse('http://localhost')),
          ),
          authSessionProvider.overrideWith(FakeAuthSessionController.new),
        ],
        child: const RondiTrackApp(),
      ),
    );
    await tester.pumpAndSettle();
    expect(find.text('Sign in to RondiTrack'), findsOneWidget);
  });
}
