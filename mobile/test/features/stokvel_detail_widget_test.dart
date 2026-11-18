import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ronditrack_mobile/features/home/data/home_repository.dart';
import 'package:ronditrack_mobile/features/stokvel/presentation/stokvel_detail_screen.dart';

void main() {
  testWidgets('detail shows cycle, contributions and role actions', (
    tester,
  ) async {
    await tester.pumpWidget(
      ProviderScope(
        retry: (retryCount, error) => null,
        overrides: [
          homeRepositoryProvider.overrideWith(
            (ref) => FakeHomeRepository(delay: Duration.zero),
          ),
        ],
        child: const MaterialApp(
          home: StokvelDetailScreen(stokvelId: 'ubuntu-savings-club'),
        ),
      ),
    );

    await tester.pumpAndSettle();

    expect(find.text('Ubuntu Savings Club'), findsOneWidget);
    expect(find.text('Current cycle'), findsOneWidget);
    expect(find.text('Contributions'), findsOneWidget);
    expect(
      find.text('Cycle and contribution details are not yet connected.'),
      findsNothing,
    );
    await tester.scrollUntilVisible(
      find.text('Close cycle'),
      250,
      scrollable: find.byType(Scrollable).first,
    );
    expect(find.text('Confirm contributions'), findsOneWidget);
    expect(find.text('Close cycle'), findsOneWidget);
  });

  testWidgets('non-member detail is presented as not found', (tester) async {
    await tester.pumpWidget(
      ProviderScope(
        retry: (retryCount, error) => null,
        overrides: [
          homeRepositoryProvider.overrideWith(
            (ref) => FakeHomeRepository(delay: Duration.zero),
          ),
        ],
        child: const MaterialApp(
          home: StokvelDetailScreen(stokvelId: 'not-a-member'),
        ),
      ),
    );

    await tester.pumpAndSettle();
    expect(find.text('Stokvel not found.'), findsOneWidget);
  });
}
