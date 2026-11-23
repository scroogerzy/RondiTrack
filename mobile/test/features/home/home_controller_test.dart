import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ronditrack_mobile/features/home/data/home_repository.dart';
import 'package:ronditrack_mobile/features/home/presentation/home_controller.dart';

void main() {
  test(
    'home uses a fresh container and the API-aligned Lerato persona',
    () async {
      final container = ProviderContainer(
        overrides: [
          homeRepositoryProvider.overrideWith(
            (ref) => FakeHomeRepository(delay: Duration.zero),
          ),
        ],
      );
      addTearDown(container.dispose);
      final result = await container.read(homeControllerProvider.future);
      expect(result.stokvels, hasLength(2));
      expect(result.stokvels.first.role, 'Treasurer');
      expect(result.stokvels.last.role, 'Member');
    },
  );

  test('Lerato has different roles in two stokvel memberships', () async {
    final repository = FakeHomeRepository(delay: Duration.zero);
    final home = await repository.loadHome();
    expect(home.stokvels.map((item) => item.role), ['Treasurer', 'Member']);
    expect(home.stokvels.map((item) => item.name), [
      'Ubuntu Savings Club',
      'Mzanzi Monthly Stokvel',
    ]);
  });

  test('empty persona has no memberships', () async {
    final home = await FakeHomeRepository(
      persona: DemoPersona.noMemberships,
      delay: Duration.zero,
    ).loadHome();
    expect(home.stokvels, isEmpty);
  });

  test(
    'detail is keyed by stokvel id and hides nonmember as missing',
    () async {
      final container = ProviderContainer(
        overrides: [
          homeRepositoryProvider.overrideWith(
            (ref) => FakeHomeRepository(delay: Duration.zero),
          ),
        ],
      );
      addTearDown(container.dispose);
      final detail = await container.read(
        stokvelDetailProvider('ubuntu-savings-club').future,
      );
      expect(detail?.homeItem.name, 'Ubuntu Savings Club');
      expect(detail?.currentCycle?.periodNumber, 5);
      expect(detail?.currentCycle?.status, 'Open');
      expect(
        await container.read(stokvelDetailProvider('not-member').future),
        isNull,
      );
    },
  );
}
