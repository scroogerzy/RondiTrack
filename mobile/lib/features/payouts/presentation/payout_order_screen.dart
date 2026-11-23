// This screen keeps related widget configuration together for readability.
// ignore_for_file: lines_longer_than_80_chars
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:ronditrack_mobile/core/theme/rondi_status_colors.dart';
import 'package:ronditrack_mobile/features/home/presentation/home_controller.dart';
import 'package:ronditrack_mobile/features/payouts/domain/payout_order.dart';
import 'package:ronditrack_mobile/features/payouts/presentation/payout_order_controller.dart';
import 'package:ronditrack_mobile/core/network/rondi_api.dart';

class PayoutOrderScreen extends ConsumerWidget {
  const new({required this.stokvelId, required this.cycleId, super.key});
  final String stokvelId;
  final String cycleId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final key = (stokvelId: stokvelId, cycleId: cycleId);
    final order = ref.watch(payoutOrderProvider(key));
    final detail = ref.watch(stokvelDetailProvider(stokvelId));
    final role = detail.asData?.value?.homeItem.role;
    final isTreasurer = role?.toLowerCase() == 'treasurer';
    return Scaffold(
      appBar: AppBar(
        title: const Text('Payout rotation'),
        leading: BackButton(onPressed: () => context.pop()),
      ),
      body: order.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (error, stack) =>
            _Failure(onRetry: () => ref.invalidate(payoutOrderProvider(key))),
        data: (value) {
          if (value == null) {
            return const Center(child: Text('Payout order not found.'));
          }
          if (value.entries.isEmpty) {
            return const Center(
              child: Text('No members are in this payout rotation yet.'),
            );
          }
          return ListView(
            padding: const EdgeInsets.all(16),
            children: [
              Text(
                'Cycle ${value.cycleNumber}',
                style: Theme.of(context).textTheme.headlineSmall,
              ),
              const SizedBox(height: 8),
              const Text(
                'The order below is supplied by RondiTrack. It is not recalculated on this device.',
              ),
              if (isTreasurer) ...[
                const SizedBox(height: 12),
                FilledButton.icon(
                  onPressed: () async {
                    try {
                      await ref.read(rondiApiProvider).recordPayout(
                        stokvelId: stokvelId,
                        cycleId: cycleId,
                      );
                      ref.invalidate(payoutOrderProvider(key));
                      ref.invalidate(stokvelDetailProvider(stokvelId));
                      ref.invalidate(homeControllerProvider);
                      if (context.mounted) {
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(content: Text('Payout recorded successfully.')),
                        );
                      }
                    } catch (error) {
                      if (context.mounted) {
                        ScaffoldMessenger.of(context).showSnackBar(
                          SnackBar(content: Text(error.toString())),
                        );
                      }
                    }
                  },
                  icon: const Icon(Icons.payments_outlined),
                  label: const Text('Record payout'),
                ),
              ],
              const SizedBox(height: 12),
              for (final entry in value.entries) _PayoutEntryTile(entry: entry),
            ],
          );
        },
      ),
    );
  }
}

class _PayoutEntryTile extends StatelessWidget {
  const new({required this.entry});
  final PayoutOrderEntry entry;

  @override
  Widget build(BuildContext context) {
    final colors =
        Theme.of(context).extension<RondiStatusColors>() ??
        RondiStatusColors.light;
    final pair = switch (entry.status) {
      PayoutStatus.next => (colors.paidOutBackground, colors.paidOutForeground),
      PayoutStatus.paid => (
        colors.contributionPaidBackground,
        colors.contributionPaidForeground,
      ),
      PayoutStatus.upcoming => (
        colors.closedBackground,
        colors.closedForeground,
      ),
      PayoutStatus.unknown => (
        Theme.of(context).colorScheme.surfaceContainerHighest,
        Theme.of(context).colorScheme.onSurface,
      ),
    };
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                CircleAvatar(child: Text('${entry.position}')),
                const SizedBox(width: 12),
                Expanded(
                  child: Text(
                    entry.memberName,
                    style: Theme.of(context).textTheme.titleMedium,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 8),
            Wrap(
              spacing: 8,
              runSpacing: 6,
              crossAxisAlignment: WrapCrossAlignment.center,
              children: [
                Text(
                  entry.amount.formatEnZa(),
                  style: Theme.of(context).textTheme.titleSmall,
                ),
                Chip(
                  label: Text(entry.status.label),
                  backgroundColor: pair.$1,
                  labelStyle: TextStyle(color: pair.$2),
                  visualDensity: VisualDensity.compact,
                ),
                Text(entry.payoutDateUtc.toLocal().toString().split(' ').first),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class _Failure extends StatelessWidget {
  const new({required this.onRetry});
  final VoidCallback onRetry;
  @override
  Widget build(BuildContext context) => Center(
    child: Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        const Text('We could not load the payout order.'),
        TextButton(onPressed: onRetry, child: const Text('Retry')),
      ],
    ),
  );
}
