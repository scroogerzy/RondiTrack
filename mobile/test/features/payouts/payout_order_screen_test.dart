import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ronditrack_mobile/core/money/cents.dart';
import 'package:ronditrack_mobile/features/home/data/home_repository.dart';
import 'package:ronditrack_mobile/features/home/domain/home_models.dart';
import 'package:ronditrack_mobile/features/home/presentation/home_controller.dart';
import 'package:ronditrack_mobile/features/payouts/domain/payout_order.dart';
import 'package:ronditrack_mobile/features/payouts/presentation/payout_order_controller.dart';
import 'package:ronditrack_mobile/features/payouts/presentation/payout_order_screen.dart';
import 'package:ronditrack_mobile/features/stokvel/domain/stokvel_detail.dart';

void main() {
  testWidgets('payout order remains readable at 360dp and 200% text', (
    tester,
  ) async {
    const key = (stokvelId: 'ubuntu-savings-club', cycleId: 'ubuntu-cycle-5');
    final detail = StokvelDetail(
      homeItem: StokvelHomeItem(
        stokvelId: key.stokvelId,
        name: 'Ubuntu Savings Club',
        role: 'Treasurer',
        currentCycle: 5,
        amountPaidThisCycleCents: const Cents(0),
        amountOwedCents: const Cents(0),
        nextPayoutDateUtc: null,
        arrearsCents: const Cents(0),
      ),
      currentUserId: 'lerato@example.com',
      currentCycle: null,
      contributions: const [],
    );
    final order = PayoutOrder(
      stokvelId: key.stokvelId,
      cycleId: key.cycleId,
      cycleNumber: 5,
      entries: [
        PayoutOrderEntry(
          memberId: 'sipho@example.com',
          memberName: 'Sipho Ndlovu With A Long Display Name',
          position: 1,
          amount: const Cents(150000),
          payoutDateUtc: DateTime.utc(2026, 11, 30),
          status: PayoutStatus.next,
        ),
      ],
    );

    await tester.binding.setSurfaceSize(const Size(360, 800));
    addTearDown(() => tester.binding.setSurfaceSize(null));
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          homeRepositoryProvider.overrideWith(
            (ref) => FakeHomeRepository(delay: Duration.zero),
          ),
          payoutOrderProvider(key).overrideWith((ref) async => order),
          stokvelDetailProvider(key.stokvelId)
              .overrideWith((ref) async => detail),
        ],
        child: const MediaQuery(
          data: MediaQueryData(
            size: Size(360, 800),
            textScaler: TextScaler.linear(2),
          ),
          child: MaterialApp(
            home: PayoutOrderScreen(
              stokvelId: 'ubuntu-savings-club',
              cycleId: 'ubuntu-cycle-5',
            ),
          ),
        ),
      ),
    );
    await tester.pumpAndSettle();

    await tester.scrollUntilVisible(
      find.text('Sipho Ndlovu With A Long Display Name'),
      250,
      scrollable: find.byType(Scrollable),
    );

    expect(find.text('Sipho Ndlovu With A Long Display Name'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });
}
