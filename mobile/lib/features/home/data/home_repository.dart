import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'package:ronditrack_mobile/core/money/cents.dart';
import 'package:ronditrack_mobile/features/home/domain/home_models.dart';
import 'package:ronditrack_mobile/features/home/domain/home_repository.dart';
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
  Future<StokvelHomeItem?> loadStokvel(String stokvelId) async {
    await _wait();
    for (final item in _homeData().stokvels) {
      if (item.stokvelId == stokvelId) return item;
    }
    return null;
  }

  HomeResponse _homeData() {
    final stokvels = switch (persona) {
      DemoPersona.lerato => [
        _item(_ubuntu, 'Ubuntu Savings Club', 'Treasurer', 4, 150000, 0, 0),
        _item(
          _mzanzi,
          'Mzanzi Monthly Stokvel',
          'Member',
          4,
          50000,
          100000,
          25000,
        ),
      ],
      DemoPersona.sibusiso => [
        _item(_ubuntu, 'Ubuntu Savings Club', 'Member', 4, 50000, 100000, 0),
      ],
      DemoPersona.thabo => [
        _item(
          _ubuntu,
          'Ubuntu Savings Club',
          'Member',
          4,
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
HomeRepository homeRepository(Ref ref) => FakeHomeRepository();
