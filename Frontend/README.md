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

On the client, the dashboard requests `GET /api/wallets/` to load all wallets, independent of the current user ID. The client generates a UUID for wallet creation with `crypto.randomUUID()` when no ID is saved, and keeps it in browser storage. Use **Generate another ID** to switch to a new ID. Funding and transfer actions also use `POST` requests. The API must allow CORS from the Next.js origin.

To create a production build, run `npm run build`; run the result with `npm start`.
