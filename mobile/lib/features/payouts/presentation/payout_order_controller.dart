import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:ronditrack_mobile/core/data/app_data_mode.dart';
import 'package:ronditrack_mobile/core/network/rondi_api.dart';
import 'package:ronditrack_mobile/features/payouts/data/api_payout_order_repository.dart';
import 'package:ronditrack_mobile/features/payouts/data/fake_payout_order_repository.dart';
import 'package:ronditrack_mobile/features/payouts/domain/payout_order.dart';
import 'package:ronditrack_mobile/features/payouts/domain/payout_order_repository.dart';

final payoutOrderRepositoryProvider = Provider<PayoutOrderRepository>((ref) {
  if (ref.watch(appDataModeProvider) == AppDataMode.api) {
    return ApiPayoutOrderRepository(ref.watch(rondiApiProvider));
  }
  return const FakePayoutOrderRepository();
});

final payoutOrderProvider =
    FutureProvider.family<PayoutOrder?, ({String stokvelId, String cycleId})>(
  (ref, key) => ref.watch(payoutOrderRepositoryProvider).load(
    key.stokvelId,
    key.cycleId,
  ),
);
