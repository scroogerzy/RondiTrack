import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ronditrack_mobile/app/app.dart';
import 'package:ronditrack_mobile/features/home/data/home_repository.dart';

void main() {
  testWidgets('RondiTrack app loads', (tester) async {
    await tester.pumpWidget(
      ProviderScope(
        retry: (retryCount, error) => null,
        overrides: [
          homeRepositoryProvider.overrideWith(
            (ref) => FakeHomeRepository(delay: Duration.zero),
          ),
        ],
        child: const RondiTrackApp(),
      ),
    );
    await tester.pumpAndSettle();
    expect(find.byType(MaterialApp), findsOneWidget);
    expect(find.text('RondiTrack'), findsOneWidget);
  });
}
