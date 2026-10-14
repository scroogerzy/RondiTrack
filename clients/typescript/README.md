# RondiTrack TypeScript API contract

This package is deliberately outside the .NET projects and pins its Node tooling to exact versions.

## Generate and compile

1. Run the API in Development with OpenAPI available at `http://localhost:5000/openapi/v1.json`.
2. From this directory run:

   ```bash
   npm install
   npm run generate
   npm run typecheck
   ```

`npm run generate` overwrites `src/generated/openapi.d.ts` from the live OpenAPI document. `npm run typecheck` compiles the home-screen example with TypeScript strict mode and emits no JavaScript.

The home-screen contract uses integer cents for money. This avoids binary floating-point currency arithmetic; callers should keep totals within JavaScript's safe-integer range.
