USE [WFTOOLS]
GO
/****** Object:  StoredProcedure [dbo].[InsertWorkflow]    Script Date: 08/28/2007 21:58:53 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[InsertWorkflow]			@TypeFullName			nvarchar(128)
												,@AssemblyFullName		nvarchar(256)
												,@IsInstanceType		bit
												,@WorkflowDefinition	ntext
												,@Activities			ntext = NULL
												,@WorkflowId			int OUTPUT
												,@Exists				bit OUTPUT
AS
 BEGIN
	SET NOCOUNT ON

	SET TRANSACTION ISOLATION LEVEL READ COMMITTED

	DECLARE @local_tran		bit
			,@error			int
			,@rowcount		int
			,@error_desc	nvarchar(256)
			,@ret			int
			,@id			int
			,@WorkflowTypeId	int

	declare @localized_string_InsertWorkflow_Failed_GetType nvarchar(256)
	set @localized_string_InsertWorkflow_Failed_GetType = N'InsertWorkflowType failed calling procedure GetTypeId'

	declare @localized_string_InsertWorkflow_Failed_WorkflowTypeInsert nvarchar(256)
	set @localized_string_InsertWorkflow_Failed_WorkflowTypeInsert = N'InsertWorkflowType failed inserting into Workflow'

	declare @localized_string_InsertWorkflow_Failed_WorkflowTypeSelect nvarchar(256)
	set @localized_string_InsertWorkflow_Failed_WorkflowTypeSelect = N'InsertWorkflowType failed selecting from Workflow'

	IF @@TRANCOUNT > 0
		SET @local_tran = 0
	ELSE
	 BEGIN
		BEGIN TRANSACTION
		SET @local_tran = 1		
	 END

	/*
		Look up or insert the type of the Workflow
		Optimized for high read to insert ratio.
		We can race between the Type table insert 
		and the Workflow table insert but we fail 
		gracefully on the Workflow insert.
	*/
	EXEC @ret = [dbo].[GetTypeId]	@TypeFullName		= @TypeFullName
									,@AssemblyFullName	= @AssemblyFullName
									,@IsInstanceType	= @IsInstanceType
									,@TypeId			= @WorkflowTypeId OUTPUT
	
	IF @@ERROR <> 0 OR @ret IS NULL OR @ret <> 0 OR @WorkflowTypeId IS NULL
	 BEGIN
		SELECT @error_desc = @localized_string_InsertWorkflow_Failed_GetType
		GOTO FAILED
	 END
	

	IF NOT EXISTS ( SELECT	1 FROM [dbo].[Workflow] WHERE [WorkflowTypeId] = @WorkflowTypeId )
	 BEGIN
		SET @Exists = 0

		INSERT [dbo].[Workflow] (
			[WorkflowTypeId]
			,[WorkflowDefinition]
		) VALUES (
			@WorkflowTypeId
			,@WorkflowDefinition
		)

		SELECT @error = @@ERROR, @rowcount = @@ROWCOUNT
	
		/*
			3604 -	Warning duplicate key ignored - does not raise exception to client
					This occurs when index specifies IGNORE_DUP_KEY
		*/
		IF @error = 3604 OR @rowcount = 0
		 BEGIN
			/*
				No need to do another lookup as the type id for the workflow is the workflow id
			*/
			SET @Exists = 1
		 END
		ELSE IF @error NOT IN ( 3604, 0 )
		 BEGIN
			/*
				If we have an error (not 0) and 
				the error number is not 3604 or 2601
				Then we have a fatal error situation
			*/
			SELECT @error_desc = @localized_string_InsertWorkflow_Failed_WorkflowTypeInsert
			GOTO FAILED
		 END
		ELSE IF @error = 0 AND @rowcount > 0 AND @Activities IS NOT NULL
		 BEGIN
			/*
				Insert was successful, insert activities
			*/
			EXEC @ret = [dbo].[InsertActivities] @WorkflowTypeId, @Activities
		 END
	 END
	ELSE
	 BEGIN
		SET @Exists = 1
	 END

	IF @local_tran = 1
		COMMIT TRANSACTION

	SELECT	@WorkflowId = @WorkflowTypeId
	SET @ret = 0
	GOTO DONE

FAILED:
	IF @local_tran = 1
		ROLLBACK TRANSACTION

	RAISERROR( @error_desc, 16, -1 )

	SET @ret = -1
	GOTO DONE

DONE:
	RETURN @ret

 END
