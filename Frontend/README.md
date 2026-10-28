# Digital Wallet frontend

This Next.js App Router dashboard uses TanStack Query to load wallets and submit wallet creation, funding, and transfer requests to the Digital Wallet API.

## Run locally

```sh
npm install
npm run dev
```

Create a `.env.local` file when the API is not running at the default URL:

```dotenv
NEXT_PUBLIC_WALLET_API_URL=http://localhost:5001
```

On the client, the dashboard requests `GET /api/wallets/` to load all wallets, independent of the current user ID. The activity and wallet transactions sections each have their own user ID dropdown and **Load user** button. The selected ID loads activity from `GET /api/users/{userId}/activity` and transactions across all of that user's wallets from `GET /api/users/{userId}/transactions`. Both dropdowns include the client-generated user ID and IDs associated with loaded wallets. The client generates a UUID for wallet creation with `crypto.randomUUID()` when no ID is saved, and keeps it in browser storage; the generated ID is selected initially. Funding and transfer actions also use `POST` requests. The API must allow CORS from the Next.js origin.

To create a production build, run `npm run build`; run the result with `npm start`.
