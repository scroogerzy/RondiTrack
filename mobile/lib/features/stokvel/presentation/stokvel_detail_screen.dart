import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:ronditrack_mobile/features/home/domain/home_models.dart';
import 'package:ronditrack_mobile/features/home/presentation/home_controller.dart';
import 'package:ronditrack_mobile/features/stokvel/domain/stokvel_status.dart';

class StokvelDetailScreen extends ConsumerWidget {
  const new({required this.stokvelId, super.key});
  final String stokvelId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final detail = ref.watch(stokvelDetailProvider(stokvelId));
    final stokvels =
        ref.watch(homeControllerProvider).asData?.value.stokvels ??
        const <StokvelHomeItem>[];
    return Scaffold(
      appBar: AppBar(
        title: const Text('Stokvel details'),
        actions: [
          PopupMenuButton<String>(
            tooltip: 'Switch stokvel',
            icon: const Icon(Icons.swap_horiz),
            onSelected: (id) => Navigator.of(context).pushReplacement(
              MaterialPageRoute<void>(
                builder: (_) => StokvelDetailScreen(stokvelId: id),
              ),
            ),
            itemBuilder: (context) => [
              for (final StokvelHomeItem item in stokvels)
                PopupMenuItem<String>(
                  value: item.stokvelId,
                  child: Text(item.name),
                ),
            ],
          ),
        ],
      ),
      body: detail.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (error, stack) => Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Text('We could not load this stokvel.'),
              TextButton(
                onPressed: () =>
                    ref.invalidate(stokvelDetailProvider(stokvelId)),
                child: const Text('Retry'),
              ),
            ],
          ),
        ),
        data: (item) {
          if (item == null) {
            return const Center(child: Text('Stokvel not found.'));
          }
          final role = StokvelRole.fromApi(item.role);
          final actions = availableActions(
            role: role,
            cycleStatus: CycleStatus.open,
            contributionStatus: ContributionStatus.unknown,
            hasOwnContribution: false,
          );
          return ListView(
            padding: const EdgeInsets.all(20),
            children: [
              Text(
                item.name,
                style: Theme.of(context).textTheme.headlineMedium,
              ),
              const SizedBox(height: 12),
              Text(
                'Your role: '
                '${role == StokvelRole.unknown ? 'Unknown' : item.role}',
              ),
              Text('Current cycle: ${item.currentCycle ?? '—'}'),
              const SizedBox(height: 16),
              const Text(
                'Cycle and contribution details are not yet connected.',
              ),
              const SizedBox(height: 16),
              Text(
                'Available actions',
                style: Theme.of(context).textTheme.titleMedium,
              ),
              for (final action in actions)
                ListTile(
                  leading: const Icon(Icons.chevron_right),
                  title: Text(switch (action) {
                    StokvelAction.pay => 'Pay contribution',
                    StokvelAction.uploadProof => 'Upload proof',
                    StokvelAction.confirm => 'Confirm contributions',
                    StokvelAction.closeCycle => 'Close cycle',
                    StokvelAction.recordPayout => 'Record payout',
                  }),
                ),
            ],
          );
        },
      ),
    );
  }
}
