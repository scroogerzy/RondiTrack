// Keep long UI expressions readable; avoid awkward line breaks.
// ignore_for_file: lines_longer_than_80_chars
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:ronditrack_mobile/core/auth/fake_auth.dart';

class SignInScreen extends ConsumerStatefulWidget {
  const new({this.from, super.key});
  final String? from;
  @override
  ConsumerState<SignInScreen> createState() => _SignInScreenState();
}

class _SignInScreenState extends ConsumerState<SignInScreen> {
  final _email = TextEditingController(text: 'lerato@example.com');
  String? _error;
  @override
  void dispose() {
    _email.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(title: const Text('Sign in to RondiTrack')),
    body: Center(
      child: ConstrainedBox(
        constraints: const BoxConstraints(maxWidth: 420),
        child: ListView(
          padding: const EdgeInsets.all(24),
          shrinkWrap: true,
          children: [
            const Icon(Icons.groups, size: 56),
            const SizedBox(height: 16),
            Text(
              'Choose a demo persona',
              style: Theme.of(context).textTheme.headlineSmall,
            ),
            const SizedBox(height: 8),
            const Text('Use one of the three seeded learner accounts.'),
            if (widget.from != null)
              const Padding(
                padding: EdgeInsets.only(top: 8),
                child: Text(
                  'After sign-in, you will return to your invitation.',
                ),
              ),
            const SizedBox(height: 20),
            TextField(
              controller: _email,
              keyboardType: TextInputType.emailAddress,
              decoration: const InputDecoration(
                labelText: 'Email address',
                border: OutlineInputBorder(),
              ),
            ),
            if (_error != null)
              Padding(
                padding: const EdgeInsets.only(top: 8),
                child: Text(
                  _error!,
                  style: TextStyle(color: Theme.of(context).colorScheme.error),
                ),
              ),
            const SizedBox(height: 16),
            FilledButton(
              onPressed: () {
                final accepted = ref
                    .read(fakeAuthProvider.notifier)
                    .signIn(_email.text);
                if (!accepted) {
                  setState(
                    () => _error = 'Use lerato@example.com, sibusiso@example.com or thabo@example.com.',
                  );
                }
              },
              child: const Text('Sign in'),
            ),
            const SizedBox(height: 12),
            const Text(
              'Demo accounts\nLerato Â· lerato@example.com\nSibusiso Â· sibusiso@example.com\nThabo Â· thabo@example.com',
            ),
          ],
        ),
      ),
    ),
  );
}
