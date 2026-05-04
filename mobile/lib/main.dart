import 'package:flutter/material.dart';
import 'services/auth_service.dart';

void main() => runApp(const RestAllApp());

class RestAllApp extends StatelessWidget {
  const RestAllApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'RestAll',
      theme: ThemeData(primarySwatch: Colors.deepOrange),
      home: const LoginPage(),
    );
  }
}

class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _auth = AuthService();
  bool _isLoading = false;

  void _login() async {
    setState(() => _isLoading = true);

    // Wywołanie serwisu logowania
    final result = await _auth.login(_emailController.text, _passwordController.text);
    bool success = result['success']; // Wyciągamy booleana z mapy

    setState(() => _isLoading = false);

    if (success) {
      // Tutaj w przyszłości dodasz nawigację: Navigator.push(...)
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Zalogowano pomyślnie!')),
      );
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Błąd logowania. Sprawdź dane.')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Padding(
        padding: const EdgeInsets.all(24.0),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Text('RestAll Mobile', style: TextStyle(fontSize: 32, fontWeight: FontWeight.bold)),
            const SizedBox(height: 40),
            TextField(
              controller: _emailController,
              decoration: const InputDecoration(labelText: 'E-mail', border: OutlineInputBorder()),
            ),
            const SizedBox(height: 16),
            TextField(
              controller: _passwordController,
              decoration: const InputDecoration(labelText: 'Hasło', border: OutlineInputBorder()),
              obscureText: true,
            ),
            const SizedBox(height: 24),
            _isLoading
                ? const CircularProgressIndicator()
                : ElevatedButton(
              onPressed: _login,
              style: ElevatedButton.styleFrom(minimumSize: const Size.fromHeight(50)),
              child: const Text('Zaloguj się'),
            ),
          ],
        ),
      ),
    );
  }
}