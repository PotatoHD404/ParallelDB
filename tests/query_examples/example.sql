﻿SELECT a.doc as 'Врач', b.c 'b'
FROM doctors as a
WHERE a = b AND c = d
   OR c = f
GROUP by c, d
HAVING f = g
UNION
SELECT 2
INTERSECT
SELECT 3
ORDER BY a.name
LIMIT 10 OFFSET 10;