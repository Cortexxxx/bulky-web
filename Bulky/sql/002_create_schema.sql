BEGIN;
CREATE TABLE public.users (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    email varchar(256) NOT NULL UNIQUE CHECK ( length(btrim(email)) > 0),
    role varchar(256) NOT NULL CHECK (role IN ('user', 'admin')) DEFAULT 'user',
    password_hash varchar(256) NOT NULL,
    password_salt varchar(256) NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);
CREATE TABLE public.sessions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL REFERENCES public.users(id) ON DELETE CASCADE,
    token_hash varchar(256) UNIQUE NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    expires_at timestamptz NOT NULL CHECK (expires_at > created_at)
);
COMMIT;