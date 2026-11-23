// Keep long UI expressions readable; avoid awkward line breaks.
// ignore_for_file: lines_longer_than_80_chars
import 'package:flutter/material.dart';

class InvitePreviewScreen extends StatefulWidget {
  const new({required this.code, super.key});
  final String code;
  @override
  State<InvitePreviewScreen> createState() => _InvitePreviewScreenState();
}

class _InvitePreviewScreenState extends State<InvitePreviewScreen> {
  bool _joined = false;
  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(title: const Text('Stokvel invitation')),
    body: Center(
      child: ConstrainedBox(
        constraints: const BoxConstraints(maxWidth: 480),
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.mail_outline, size: 56),
              const SizedBox(height: 16),
              Text(
                'You are invited to join a stokvel',
                style: Theme.of(context).textTheme.headlineSmall,
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 8),
              Text('Invite code: ${widget.code}'),
              const SizedBox(height: 20),
              if (_joined)
                const Text(
                  'Invite accepted in this demo. Week 11 will connect the real join API.',
                )
              else
                FilledButton(
                  onPressed: () => setState(() => _joined = true),
                  child: const Text('Join stokvel (demo)'),
                ),
            ],
          ),
        ),
      ),
    ),
  );
}
