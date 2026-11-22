// ignore_for_file: lines_longer_than_80_chars
import 'package:ronditrack_mobile/core/money/cents.dart';
import 'package:ronditrack_mobile/features/payouts/domain/payout_order.dart';
import 'package:ronditrack_mobile/features/payouts/domain/payout_order_repository.dart';

/// Fixed API-shaped fixture. It deliberately does not calculate or sort rotation.
class FakePayoutOrderRepository implements PayoutOrderRepository {
  const FakePayoutOrderRepository();

  @override
  Future<PayoutOrder?> load(String stokvelId, String cycleId) async {
    if (!{
      'ubuntu-savings-club:ubuntu-cycle-5',
      'mzanzi-monthly-stokvel:mzanzi-cycle-5',
    }.contains('$stokvelId:$cycleId'))
      return null;

    final ubuntu = stokvelId == 'ubuntu-savings-club';
    final date = DateTime.utc(2026, 11, 30, 23, 59, 59);
    return PayoutOrder(
      stokvelId: stokvelId,
      cycleId: cycleId,
      cycleNumber: 5,
      entries: [
        PayoutOrderEntry(
          memberId: 'lerato@example.com',
          memberName: 'Lerato Dlamini',
          position: 1,
          amount: Cents(ubuntu ? 150000 : 100000),
          payoutDateUtc: date,
          status: PayoutStatus.upcoming,
        ),
        PayoutOrderEntry(
          memberId: 'sipho@example.com',
          memberName: 'Sipho Ndlovu',
          position: 2,
          amount: Cents(ubuntu ? 150000 : 100000),
          payoutDateUtc: date,
          status: PayoutStatus.next,
        ),
        PayoutOrderEntry(
          memberId: 'thabo@example.com',
          memberName: 'Thabo Mokoena',
          position: 3,
          amount: Cents(ubuntu ? 150000 : 100000),
          payoutDateUtc: date,
          status: PayoutStatus.upcoming,
        ),
      ],
    );
  }
}
