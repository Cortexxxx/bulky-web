CREATE OR REPLACE FUNCTION public.get_user_by_email(p_email varchar(256))
RETURNS TABLE(id uuid, email varchar(256), role varchar(256), 
              password_hash varchar(256), password_salt varchar(256), created_at timestamptz)
LANGUAGE sql STABLE AS
$$
    SELECT u.id, u.email, u.role, u.password_hash, u.password_salt, u.created_at
    FROM public.users AS u
    WHERE u.email = p_email
$$;

CREATE OR REPLACE FUNCTION public.get_session_by_token(p_token varchar(256))
    RETURNS TABLE(id uuid, user_id uuid, token varchar(256), created_at timestamptz, expires_at timestamptz)
    LANGUAGE sql STABLE AS
$$
SELECT s.id, s.user_id, s.token, s.created_at, s.expires_at
FROM public.sessions AS s   
WHERE s.token = p_token AND s.expires_at > now()
$$;
