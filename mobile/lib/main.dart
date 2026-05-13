import 'package:flutter/material.dart';
import 'services/auth_service.dart';
import 'screens/waiter/tables_screen.dart';

void main() => runApp(const RestAllApp());

class RestAllApp extends StatelessWidget {
  const RestAllApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'RestAll',
      theme: ThemeData(
        primaryColor: Colors.deepOrange,
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.deepOrange),
        useMaterial3: true, // Nowoczesny wygląd Material 3
        inputDecorationTheme: InputDecorationTheme(
          filled: true,
          fillColor: Colors.grey[100],
          border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
        ),
        elevatedButtonTheme: ElevatedButtonThemeData(
          style: ElevatedButton.styleFrom(
            backgroundColor: Colors.deepOrange,
            foregroundColor: Colors.white,
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),          ),
        ),
      ),
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

    // 1. Wywołanie serwisu
    final result = await _auth.login(
        _emailController.text,
        _passwordController.text
    );

    // 2. Zatrzymanie ładowania (Musi być przed nawigacją lub po błędzie)
    setState(() => _isLoading = false);

    // 3. Obsługa wyniku
    if (result['success'] == true) {
      // Sukces: Nawigacja do ekranu stolików
      if (!mounted) return; // Dobra praktyka we Flutterze
      Navigator.push(
        context,
        MaterialPageRoute(builder: (context) => const TablesScreen()),
      );
    } else {
      // Błąd: Pokazujemy SnackBar z informacją co poszło nie tak
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Błąd: ${result['error'] ?? 'Nieznany błąd'}'),
          backgroundColor: Colors.red,
        ),
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