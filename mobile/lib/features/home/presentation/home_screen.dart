import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:ronditrack_mobile/features/home/presentation/home_controller.dart';
import 'package:ronditrack_mobile/features/stokvel/presentation/stokvel_detail_screen.dart';

class HomeScreen extends ConsumerWidget {
  // This widget needs a named const constructor for const call sites.
  // ignore: unnecessary_type_name_in_constructor
  const HomeScreen({super.key});

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
                  onPressed: () {
                    ref.invalidate(homeControllerProvider);
                  },
                  child: const Text('Retry'),
                ),
              ),
            ],
          ),
          data: (data) {
            if (data.stokvels.isEmpty) {
              return ListView(
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
              );
            }

            return ListView(
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
                      subtitle: Text(
                        '${item.role} · '
                        'Cycle ${item.currentCycle ?? '—'}\n'
                        'Paid '
                        '${item.amountPaidThisCycleCents.formatEnZa()}  '
                        'Owed '
                        '${item.amountOwedCents.formatEnZa()}\n'
                        'Arrears '
                        '${item.arrearsCents.formatEnZa()}\n'
                        'Next payout '
                        '${_nextPayoutLabel(item.nextPayoutDateUtc)}',
                      ),
                      isThreeLine: true,
                      trailing: const Icon(Icons.chevron_right),
                      onTap: () {
                        Navigator.of(context).push(
                          MaterialPageRoute<void>(
                            builder: (_) =>
                                StokvelDetailScreen(stokvelId: item.stokvelId),
                          ),
                        );
                      },
                    ),
                  ),
              ],
            );
          },
        ),
      ),
    );
  }
}

String _nextPayoutLabel(DateTime? date) {
  return date?.toLocal().toString().split(' ').first ?? 'Not scheduled';
}
