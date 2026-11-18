import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:ronditrack_mobile/core/auth/fake_auth.dart';

class ProfileScreen extends ConsumerWidget {
  const new({super.key});
  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final auth = ref.watch(fakeAuthProvider);
    return Scaffold(
      appBar: AppBar(title: const Text('Profile')),
      body: ListView(
        padding: const EdgeInsets.all(20),
        children: [
          const CircleAvatar(radius: 36, child: Icon(Icons.person, size: 36)),
          const SizedBox(height: 16),
          Text(
            auth.displayName,
            style: Theme.of(context).textTheme.headlineSmall,
          ),
          Text(auth.email ?? 'Not signed in'),
          const SizedBox(height: 24),
          FilledButton.tonalIcon(
            onPressed: () => ref.read(fakeAuthProvider.notifier).signOut(),
            icon: const Icon(Icons.logout),
            label: const Text('Sign out'),
          ),
        ],
      ),
    );
  }
}
