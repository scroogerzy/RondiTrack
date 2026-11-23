import 'package:flutter_test/flutter_test.dart';
import 'package:ronditrack_mobile/features/home/data/home_repository.dart';

void main() {
  test('detail includes cycle totals and contribution records', () async {
    final repository = FakeHomeRepository(delay: Duration.zero);
    final detail = await repository.loadStokvel('ubuntu-savings-club');

    expect(detail, isNotNull);
    expect(detail!.currentCycle!.periodNumber, 5);
    expect(detail.currentCycle!.totalCollectedCents, 100000);
    expect(detail.contributions, isNotEmpty);
    expect(detail.hasOwnContribution, isFalse);
  });

  test(
    'member detail identifies own contribution by caller identity',
    () async {
      final repository = FakeHomeRepository(
        persona: DemoPersona.sibusiso,
        delay: Duration.zero,
      );
      final detail = await repository.loadStokvel('ubuntu-savings-club');

      expect(detail, isNotNull);
      expect(detail!.hasOwnContribution, isTrue);
      expect(detail.ownContributionCents, 50000);
    },
  );

  test('non-member and unknown stokvel both return null', () async {
    final repository = FakeHomeRepository(delay: Duration.zero);
    expect(await repository.loadStokvel('not-a-member'), isNull);
  });
}
