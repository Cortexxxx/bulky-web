BEGIN;

ALTER TABLE public.users ADD COLUMN username varchar(256);

UPDATE public.users
SET username = 'user-' || left(id::text, 8);

ALTER TABLE public.users
    ALTER COLUMN username SET NOT NULL,
    ADD CONSTRAINT users_username_min_length CHECK (char_length(btrim(username)) >= 3);

COMMIT;
