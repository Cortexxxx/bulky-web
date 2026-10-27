CREATE OR REPLACE PROCEDURE public.create_user(
    IN p_email varchar(256),
    IN p_password_hash varchar(256),
    IN p_password_salt varchar(256),
    OUT p_id uuid
)
LANGUAGE plpgsql AS
    $$
        BEGIN 
            INSERT INTO public.users(email, password_hash, password_salt) 
            VALUES (p_email, p_password_hash, p_password_salt)
            RETURNING id INTO p_id;
        end;
    $$;

CREATE OR REPLACE PROCEDURE public.create_session(
    IN p_user_id uuid,
    IN p_token varchar(256),
    IN p_expires_at timestamptz,
    OUT p_id uuid
)
    LANGUAGE plpgsql AS
$$
BEGIN
    INSERT INTO public.sessions(user_id, token, expires_at)
    VALUES (p_user_id, p_token, p_expires_at)
    RETURNING id INTO p_id;
end;
$$; 

CREATE OR REPLACE PROCEDURE public.update_session_time(
    IN p_id uuid,
    IN p_expires_at timestamptz,
    OUT p_found bool
)
    LANGUAGE plpgsql AS
$$
BEGIN
    UPDATE public.sessions AS s
    SET expires_at = p_expires_at
    WHERE s.id = p_id;
    p_found := FOUND;
end;
$$;

CREATE OR REPLACE PROCEDURE public.delete_session_by_id(
    IN p_id uuid,
    OUT p_found bool
)
    LANGUAGE plpgsql AS
$$
BEGIN
    DELETE FROM public.sessions as s
    WHERE s.id = p_id;
    p_found := FOUND;
end;
$$;

CREATE OR REPLACE PROCEDURE public.delete_user_sessions(
    IN p_user_id uuid,
    OUT p_found bool
)
    LANGUAGE plpgsql AS
$$
BEGIN
    DELETE FROM public.sessions as s
    WHERE s.user_id = p_user_id;
    p_found := FOUND;
end;
$$;

CREATE OR REPLACE PROCEDURE public.delete_expired_sessions(
    OUT p_found bool
)
    LANGUAGE plpgsql AS
$$
BEGIN
    DELETE FROM public.sessions as s
    WHERE s.expires_at < now();
    p_found := FOUND;
end;
$$;