// Keep long UI expressions readable; avoid awkward line breaks.
// ignore_for_file: lines_longer_than_80_chars
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:ronditrack_mobile/core/theme/rondi_status_colors.dart';
import 'package:ronditrack_mobile/features/home/presentation/home_controller.dart';
import 'package:ronditrack_mobile/features/navigation/app_routes.dart';

class HomeScreen extends ConsumerWidget {
  const new({super.key});
  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final home = ref.watch(homeControllerProvider);
    return Scaffold(
      appBar: AppBar(title: const Text('RondiTrack')),
      body: RefreshIndicator(
        onRefresh: () => ref.read(homeControllerProvider.notifier).refresh(),
        child: home.when(
          loading: () => ListView(
            children: const [
              SizedBox(height: 220),
              Center(child: CircularProgressIndicator()),
            ],
          ),
          error: (error, stack) => ListView(
            children: [
              const SizedBox(height: 140),
              const Center(child: Text('We could not load your stokvels.')),
              Center(
                child: TextButton(
                  onPressed: () => ref.invalidate(homeControllerProvider),
                  child: const Text('Retry'),
                ),
              ),
            ],
          ),
          data: (data) => data.stokvels.isEmpty
              ? ListView(
                  padding: const EdgeInsets.all(24),
                  children: const [
                    SizedBox(height: 100),
                    Icon(Icons.groups_outlined, size: 56),
                    SizedBox(height: 16),
                    Center(child: Text('You have no stokvel memberships yet.')),
                    Center(
                      child: Text('Ask your stokvel treasurer to invite you.'),
                    ),
                  ],
                )
              : ListView(
                  padding: const EdgeInsets.all(16),
                  children: [
                    Text(
                      'Your stokvels',
                      style: Theme.of(context).textTheme.headlineSmall,
                    ),
                    for (final item in data.stokvels)
                      Card(
                        child: ListTile(
                          title: Text(item.name),
                          subtitle: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                '${item.role} Â· Cycle ${item.currentCycle ?? 'â€”'}\nPaid ${item.amountPaidThisCycleCents.formatEnZa()} Â· Owed ${item.amountOwedCents.formatEnZa()}\nNext payout: ${item.nextPayoutDateUtc?.toLocal().toString().split(' ').first ?? 'Not scheduled'}',
                              ),
                              if (item.arrearsCents.value > 0)
                                _ArrearsChip(
                                  amount: item.arrearsCents.formatEnZa(),
                                ),
                            ],
                          ),
                          trailing: const Icon(Icons.chevron_right),
                          onTap: () =>
                              StokvelDetailRoute(id: item.stokvelId)
                                  .push<void>(context),
                        ),
                      ),
                  ],
                ),
        ),
      ),
    );
  }
}

class _ArrearsChip extends StatelessWidget {
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
