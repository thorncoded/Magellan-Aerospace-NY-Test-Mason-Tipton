-- Create a database called Part, the date can be formatted later but type DATE is stored as YYYY-MM-DD.
CREATE TABLE Part( 
    id SERIAL NOT NULL PRIMARY KEY,
    item_name VARCHAR(50) NOT NULL, 
    parent_item INTEGER REFERENCES Part(id),
    cost INTEGER NOT NULL,
    req_date DATE NOT NULL 
); 

-- Insert some data into the table
INSERT INTO Part (item_name, cost, req_date) VALUES ('Item1', 500, '2024-02-20');

INSERT INTO Part (item_name, parent_item, cost, req_date) VALUES ('Sub1', 1, 200, '2024-02-10');
INSERT INTO Part (item_name, parent_item, cost, req_date) VALUES ('Sub2', 1, 300, '2024-01-05');

INSERT INTO Part (item_name, parent_item, cost, req_date) VALUES ('Sub3', 2, 300, '2024-01-02');
INSERT INTO Part (item_name, parent_item, cost, req_date) VALUES ('Sub4', 2, 400, '2024-01-02');

INSERT INTO Part (item_name, cost, req_date) VALUES ('Item2', 600, '2024-03-15');
INSERT INTO Part (item_name, parent_item, cost, req_date) VALUES ('Sub1', 6, 200, '2024-02-25');


-- finding out

CREATE OR REPLACE FUNCTION Get_Total_Cost(item_name_searching VARCHAR(50)) 
RETURNS INTEGER AS
$$
DECLARE
    total_cost INTEGER := 0;
BEGIN
    -- Check if the parent_item of the specified item_name_searching is not null
    IF EXISTS (
        SELECT 1 FROM Part WHERE item_name = item_name_searching AND parent_item IS NOT NULL
    ) THEN
        RETURN NULL;
    END IF;

    -- fun w/recursion! >:)
    WITH RECURSIVE item_hierarchy AS (
        SELECT id, cost, parent_item
        FROM Part
        WHERE item_name = item_name_searching
        UNION ALL
        SELECT p.id, p.cost, p.parent_item
        FROM Part p
        JOIN item_hierarchy ih ON p.parent_item = ih.id
    )

    -- was putting this in the wrong place before
    SELECT COALESCE(SUM(cost), 0) INTO total_cost
    FROM item_hierarchy;

    RETURN total_cost;
END;
$$
LANGUAGE plpgsql;






