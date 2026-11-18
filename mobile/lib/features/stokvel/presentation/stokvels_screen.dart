// Keep long UI expressions readable; avoid awkward line breaks.
// ignore_for_file: lines_longer_than_80_chars
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:ronditrack_mobile/core/theme/rondi_status_colors.dart';
import 'package:ronditrack_mobile/features/home/domain/home_models.dart';
import 'package:ronditrack_mobile/features/home/presentation/home_controller.dart';
import 'package:ronditrack_mobile/features/navigation/app_routes.dart';

class StokvelsScreen extends ConsumerWidget {
  const new({this.showAppBar = true, super.key});
  final bool showAppBar;
  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final home = ref.watch(homeControllerProvider);
    return Scaffold(
      appBar: showAppBar ? AppBar(title: const Text('Stokvels')) : null,
      body: home.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (error, stack) => Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Text('We could not load your stokvels.'),
              TextButton(
                onPressed: () => ref.invalidate(homeControllerProvider),
                child: const Text('Retry'),
              ),
            ],
          ),
        ),
        data: (data) => data.stokvels.isEmpty
            ? const Center(
                child: Padding(
                  padding: EdgeInsets.all(24),
                  child: Text('You have no stokvel memberships yet.'),
                ),
              )
            : ListView(
                padding: const EdgeInsets.all(16),
                children: [
                  for (final item in data.stokvels) StokvelListTile(item: item),
                ],
              ),
      ),
    );
  }
}

class StokvelListTile extends StatelessWidget {
  const new({required this.item, super.key});
  final StokvelHomeItem item;
  @override
  Widget build(BuildContext context) => Card(
    child: ListTile(
      title: Text(item.name),
      subtitle: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            '${item.role} Â· Cycle ${item.currentCycle ?? 'â€”'}\nPaid ${item.amountPaidThisCycleCents.formatEnZa()} Â· Owed ${item.amountOwedCents.formatEnZa()}\nNext payout: ${item.nextPayoutDateUtc?.toLocal().toString().split(' ').first ?? 'Not scheduled'}',
          ),
          if (item.arrearsCents.value > 0)
            _StokvelArrearsChip(amount: item.arrearsCents.formatEnZa()),
        ],
      ),
      trailing: const Icon(Icons.chevron_right),
      onTap: () => StokvelDetailRoute(id: item.stokvelId).push<void>(context),
    ),
  );
}

class _StokvelArrearsChip extends StatelessWidget {
  const new({required this.amount});
  final String amount;
  @override
  Widget build(BuildContext context) {
    final tokens = Theme.of(context).extension<RondiStatusColors>()!;
    return Chip(
      label: Text('Arrears $amount'),
      backgroundColor: tokens.arrearsBackground,
      labelStyle: TextStyle(color: tokens.arrearsForeground),
    );
  }
}
