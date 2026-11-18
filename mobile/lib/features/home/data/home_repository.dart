import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'package:ronditrack_mobile/core/auth/fake_auth.dart';
import 'package:ronditrack_mobile/core/money/cents.dart';
import 'package:ronditrack_mobile/features/home/domain/home_models.dart';
import 'package:ronditrack_mobile/features/home/domain/home_repository.dart';
import 'package:ronditrack_mobile/features/stokvel/domain/stokvel_detail.dart';

part 'home_repository.g.dart';

enum DemoPersona { lerato, sibusiso, thabo, noMemberships }

class FakeHomeRepository implements HomeRepository {
  new({
    this.persona = DemoPersona.lerato,
    this.delay = const Duration(milliseconds: 250),
  });

  final DemoPersona persona;
  final Duration delay;

  static const _ubuntu = 'ubuntu-savings-club';
  static const _mzanzi = 'mzanzi-monthly-stokvel';

  Future<void> _wait() => Future<void>.delayed(delay);

  @override
  Future<HomeResponse> loadHome() async {
    await _wait();
    return _homeData();
  }

  @override
  Future<StokvelDetail?> loadStokvel(String stokvelId) async {
    await _wait();
    final home = _homeData();
    StokvelHomeItem? item;
    for (final candidate in home.stokvels) {
      if (candidate.stokvelId == stokvelId) {
        item = candidate;
        break;
      }
    }
    if (item == null) return null;

    final isUbuntu = stokvelId == _ubuntu;
    final cycle = StokvelCycle(
      id: isUbuntu ? 'ubuntu-cycle-5' : 'mzanzi-cycle-5',
      periodNumber: item.currentCycle ?? 5,
      startDateUtc: DateTime.utc(2026, 11),
      endDateUtc: DateTime.utc(2026, 11, 30, 23, 59, 59),
      targetAmountCents: isUbuntu ? 150000 : 100000,
      status: 'Open',
      totalCollectedCents: isUbuntu ? 100000 : 50000,
      amountDueCents: isUbuntu ? 150000 : 100000,
    );
    final contributions = <StokvelContribution>[
      StokvelContribution(
        id: 'contribution-thabo-$stokvelId',
        userId: 'thabo@example.com',
        cycleId: cycle.id,
        amountCents: 50000,
        recordedAtUtc: DateTime.utc(2026, 11, 5),
      ),
      StokvelContribution(
        id: 'contribution-sibusiso-$stokvelId',
        userId: 'sibusiso@example.com',
        cycleId: cycle.id,
        amountCents: 50000,
        recordedAtUtc: DateTime.utc(2026, 11, 6),
      ),
    ];
    return StokvelDetail(
      homeItem: item,
      currentUserId: home.currentUserId,
      currentCycle: cycle,
      contributions: contributions,
    );
  }

  HomeResponse _homeData() {
    final stokvels = switch (persona) {
      DemoPersona.lerato => [
        _item(_ubuntu, 'Ubuntu Savings Club', 'Treasurer', 5, 150000, 0, 0),
        _item(
          _mzanzi,
          'Mzanzi Monthly Stokvel',
          'Member',
          5,
          50000,
          100000,
          25000,
        ),
      ],
      DemoPersona.sibusiso => [
        _item(_ubuntu, 'Ubuntu Savings Club', 'Member', 5, 50000, 100000, 0),
      ],
      DemoPersona.thabo => [
        _item(
          _ubuntu,
          'Ubuntu Savings Club',
          'Member',
          5,
          50000,
          100000,
          15000,
        ),
      ],
      DemoPersona.noMemberships => <StokvelHomeItem>[],
    };
    return HomeResponse(
      currentUserId: switch (persona) {
        DemoPersona.lerato => 'lerato@example.com',
        DemoPersona.sibusiso => 'sibusiso@example.com',
        DemoPersona.thabo => 'thabo@example.com',
        DemoPersona.noMemberships => 'empty-persona',
      },
      stokvels: stokvels,
    );
  }

  static StokvelHomeItem _item(
    String id,
    String name,
    String role,
    int? cycle,
    int paid,
    int owed,
    int arrears,
  ) => StokvelHomeItem(
    stokvelId: id,
    name: name,
    role: role,
    currentCycle: cycle,
    amountPaidThisCycleCents: Cents(paid),
    amountOwedCents: Cents(owed),
    nextPayoutDateUtc: null,
    arrearsCents: Cents(arrears),
  );
}

@Riverpod(keepAlive: true)
HomeRepository homeRepository(Ref ref) {
  final email = ref.watch(fakeAuthProvider).email;
  final persona = switch (email) {
    'sibusiso@example.com' => DemoPersona.sibusiso,
    'thabo@example.com' => DemoPersona.thabo,
    null => DemoPersona.noMemberships,
    _ => DemoPersona.lerato,
  };
  return FakeHomeRepository(persona: persona);
}
