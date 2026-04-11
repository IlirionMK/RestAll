/** @type {import('tailwindcss').Config} */
export default {
    darkMode: 'class',
    content: [
        "./index.html",
        "./src/**/*.{vue,js,ts,jsx,tsx}",
    ],
    theme: {
        extend: {
            colors: {
                restall: {
                    light: '#FAF9F6',
                    dark: '#1F2937',
                    green: '#14532D',
                    gold: '#C29B40',
                }
            },
            borderRadius: {
                'squircle': '1.5rem',
            }
        },
    },
    plugins: [],
}