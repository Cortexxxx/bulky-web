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

CREATE TABLE public.muscle_groups (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    title varchar(256) NOT NULL UNIQUE,
    description varchar(500) NOT NULL,
    image_path varchar(256),
    is_archived boolean NOT NULL DEFAULT FALSE
);
CREATE TABLE public.exercises (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    title varchar(256) NOT NULL UNIQUE,
    description varchar(500) NOT NULL,
    image_path varchar(256),
    is_archived boolean NOT NULL DEFAULT FALSE
);
CREATE TABLE public.exercise_muscle_groups (
    PRIMARY KEY (exercise_id, muscle_group_id),
    role varchar(256) NOT NULL CHECK (role in ('primary', 'secondary')) DEFAULT 'primary',
    exercise_id uuid NOT NULL REFERENCES public.exercises ON DELETE CASCADE,
    muscle_group_id uuid NOT NULL REFERENCES public.muscle_groups ON DELETE CASCADE
);

CREATE TABLE public.workouts (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL REFERENCES public.users ON DELETE CASCADE,
    title varchar(256) NOT NULL,
    started_at timestamptz NOT NULL DEFAULT now(),
    ended_at timestamptz CHECK ( started_at < workouts.ended_at )
);

CREATE TABLE public.sets (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    set_number smallint NOT NULL check ( set_number > 0 ),
    exercise_number smallint NOT NULL check ( exercise_number > 0 ),
    repeats smallint NOT NULL CHECK ( repeats > 0 ),
    weight numeric(5, 2) NOT NULL CHECK ( weight > 0 ),
    workout_id uuid NOT NULL REFERENCES public.workouts ON DELETE CASCADE,
    exercise_id uuid NOT NULL REFERENCES public.exercises,
    UNIQUE (workout_id, exercise_id, set_number),
    UNIQUE (workout_id, set_number, exercise_number)
);
CREATE INDEX idx_exercise_muscle_groups_muscle_group_id ON public.exercise_muscle_groups(muscle_group_id);
COMMIT;