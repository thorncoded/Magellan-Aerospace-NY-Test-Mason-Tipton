-- need to drop, cannot do "create or alter" type syntax iirc, check this later.
DROP DATABASE IF EXISTS Part;


-- Create a database called Part, 
CREATE DATABASE Part;


\c part

--DROP TABLE IF EXISTS public.item;

-- the date can be formatted later but type DATE is stored as YYYY-MM-DD.

CREATE TABLE public.item( 
    id SERIAL NOT NULL PRIMARY KEY,
    item_name VARCHAR(50) NOT NULL, 
    parent_item INTEGER REFERENCES item(id),
    cost INTEGER NOT NULL,
    req_date DATE NOT NULL 
); 

-- Insert some data into the table
INSERT INTO public.item (item_name, cost, req_date) VALUES ('Item1', 500, '2024-02-20');

INSERT INTO public.item (item_name, parent_item, cost, req_date) VALUES ('Sub1', 1, 200, '2024-02-10');
INSERT INTO public.item (item_name, parent_item, cost, req_date) VALUES ('Sub2', 1, 300, '2024-01-05');

INSERT INTO public.item(item_name, parent_item, cost, req_date) VALUES ('Sub3', 2, 300, '2024-01-02');
INSERT INTO public.item (item_name, parent_item, cost, req_date) VALUES ('Sub4', 2, 400, '2024-01-02');

INSERT INTO public.item(item_name, cost, req_date) VALUES ('Item2', 600, '2024-03-15');
INSERT INTO public.item(item_name, parent_item, cost, req_date) VALUES ('Sub1', 6, 200, '2024-02-25');


-- finding out

CREATE OR REPLACE FUNCTION Get_Total_Cost(item_name_searching VARCHAR(50)) 
RETURNS INTEGER AS
$$
DECLARE
    total_cost INTEGER := 0;
BEGIN
    -- Check if the parent_item of the specified item_name_searching is not null
    IF EXISTS (
        SELECT 1 FROM public.item WHERE item_name = item_name_searching AND parent_item IS NOT NULL
    ) THEN
        RETURN NULL;
    END IF;

    -- fun w/recursion! >:)
    WITH RECURSIVE item_hierarchy AS (
        SELECT id, cost, parent_item
        FROM public.item
        WHERE item_name = item_name_searching
        UNION ALL
        SELECT p.id, p.cost, p.parent_item
        FROM item p
        JOIN item_hierarchy ih ON p.parent_item = ih.id
    )

    -- was putting this in the wrong place before
    SELECT COALESCE(SUM(cost), 0) INTO total_cost
    FROM item_hierarchy;

    RETURN total_cost;
END;
$$
LANGUAGE plpgsql;

--for our connection strings in the controller class

CREATE ROLE dbuser WITH LOGIN PASSWORD 'magellan';
GRANT CONNECT ON DATABASE Part TO dbuser;
GRANT USAGE ON SCHEMA public TO dbuser;
GRANT INSERT ON item TO dbuser;
GRANT SELECT ON item TO dbuser;
GRANT UPDATE ON item TO dbuser;
GRANT DELETE ON item TO dbuser;
