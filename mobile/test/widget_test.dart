import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ronditrack_mobile/app/app.dart';

void main() {
  testWidgets('RondiTrack app gates signed-out users', (tester) async {
    await tester.pumpWidget(
      ProviderScope(
        retry: (retryCount, error) => null,
        child: const RondiTrackApp(),
      ),
    );
    await tester.pumpAndSettle();
    expect(find.text('Sign in to RondiTrack'), findsOneWidget);
  });
}
