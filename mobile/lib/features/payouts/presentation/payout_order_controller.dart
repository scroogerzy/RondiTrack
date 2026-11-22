// ignore_for_file: lines_longer_than_80_chars
// ignore_for_file: lines_longer_than_80_chars
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:ronditrack_mobile/features/payouts/data/fake_payout_order_repository.dart';
import 'package:ronditrack_mobile/features/payouts/domain/payout_order.dart';
import 'package:ronditrack_mobile/features/payouts/domain/payout_order_repository.dart';
import 'package:ronditrack_mobile/features/home/data/home_repository.dart';

final payoutOrderRepositoryProvider = Provider<PayoutOrderRepository>(
  (ref) => const FakePayoutOrderRepository(),
);

/// The record key gives stable family identity for the stokvel/cycle pair.
final payoutOrderProvider =
    FutureProvider.family<PayoutOrder?, ({String stokvelId, String cycleId})>((
      ref,
      key,
    ) async {
      final home = await ref.watch(homeRepositoryProvider).loadHome();
      if (!home.stokvels.any((item) => item.stokvelId == key.stokvelId))
        return null;
      return ref
          .watch(payoutOrderRepositoryProvider)
          .load(key.stokvelId, key.cycleId);
    });
