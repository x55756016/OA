DELIMITER $$

DROP PROCEDURE IF EXISTS InsertCompletedScope $$
CREATE PROCEDURE InsertCompletedScope
(
  IN p_INSTANCE_ID CHAR(36)
  ,IN p_COMPLETED_SCOPE_ID CHAR(36)
  ,IN p_STATE BLOB
)
BEGIN
  DECLARE l_NOW DATETIME DEFAULT UTC_TIMESTAMP();

  UPDATE COMPLETED_SCOPE
  SET
    STATE = p_STATE
    ,MODIFIED = l_NOW
  WHERE
    COMPLETED_SCOPE_ID = p_COMPLETED_SCOPE_ID;

  IF ROW_COUNT() = 0 THEN
    INSERT INTO COMPLETED_SCOPE
    (
      INSTANCE_ID, COMPLETED_SCOPE_ID
      ,STATE, MODIFIED
    )
    VALUES
    (
      p_INSTANCE_ID, p_COMPLETED_SCOPE_ID
      ,p_STATE, l_NOW
    );
  END IF;
END $$

DELIMITER ;