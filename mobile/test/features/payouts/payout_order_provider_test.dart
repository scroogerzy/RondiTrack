// ignore_for_file: lines_longer_than_80_chars
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:ronditrack_mobile/core/auth/fake_auth.dart';
import 'package:ronditrack_mobile/features/payouts/presentation/payout_order_controller.dart';

void main() {
  test(
    'payout family is membership-scoped and refreshes on persona change',
    () async {
      final container = ProviderContainer();
      addTearDown(container.dispose);
      const key = (stokvelId: 'ubuntu-savings-club', cycleId: 'ubuntu-cycle-5');

      container.read(fakeAuthProvider.notifier).signIn('lerato@example.com');
      final leratoOrder = await container.read(payoutOrderProvider(key).future);
      expect(leratoOrder, isNotNull);

      container.read(fakeAuthProvider.notifier).signIn('thabo@example.com');
      final thaboOrder = await container.read(payoutOrderProvider(key).future);
      expect(thaboOrder, isNull);
    },
  );
}
