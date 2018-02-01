#!groovy

def buildVersion = ""

node {
	echo "$env.WORKSPACE"
	def commonReleaseBuildParams = "/p:Configuration=Release;Platform=AnyCPU /p:CI_BuildNum=${env.BUILD_ID}"
    def solutionLocation = "$env.WORKSPACE/StarChef.MSMQService.sln";
    def buildMode = "Release";
	def msbuild = tool name:'MSBUILD 15', type:'msbuild'

    stage ('Get Source Code') {
		checkout([
			$class: 'GitSCM',
			branches: scm.branches,
			extensions: scm.extensions + [[$class: 'CleanCheckout']],
			userRemoteConfigs: scm.userRemoteConfigs
		])
		echo "Retrieved $env.BRANCH_NAME in workspace: $env.WORKSPACE"
	}

	stage ('Initialize Workspace') {
	 	echo 'Restore nuget packages for solution'
		bat "\"$env.WORKSPACE/Tools/nuget.exe\" restore \"$solutionLocation\""
	 	
		echo 'Calculate Version'
		bat "\"$msbuild\" ./StarChef.MSMQService/StarChef.MSMQService.csproj /t:GenVersionOutputFile /p:OutDir=\"$env.WORKSPACE\" $commonReleaseBuildParams"
		buildVersion = getVersion("$env.WORKSPACE/Version.txt")
		currentBuild.displayName = '# ' + buildVersion
	}

    stage ('Build') {
		echo 'Build Start'
		bat "\"${msbuild}\" \"${solutionLocation}\" /t:Rebuild $commonReleaseBuildParams /p:Platform=\"Any CPU\""
	}

	stage('Run Unit Tests'){
		echo 'Run Unit Tests StarChef.Orchestrate'
		bat "\"$env.WORKSPACE/%XUNIT_CONSOLE_RUNNER_V2_3_1%\" \"$env.WORKSPACE/StarChef.Orchestrate.Tests/bin/${buildMode}/StarChef.Orchestrate.Tests.dll\" -xml \"$env.WORKSPACE/starchef-orchestrate-xunit-results.xml\""
    	step([$class: 'XUnitPublisher', testTimeMargin: '3000', thresholdMode: 0, thresholds: [[$class: 'FailedThreshold', failureNewThreshold: '', failureThreshold: '', unstableNewThreshold: '', unstableThreshold: ''], [$class: 'SkippedThreshold', failureNewThreshold: '', failureThreshold: '', unstableNewThreshold: '', unstableThreshold: '']], tools: [[$class: 'XUnitDotNetTestType', deleteOutputFiles: true, failIfNotNew: true, pattern: '**/starchef-orchestrate-xunit-results.xml', skipNoTestFiles: false, stopProcessingIfError: true]]])

		echo 'Run Unit Tests StarChef.Listener'
		bat "\"$env.WORKSPACE/%XUNIT_CONSOLE_RUNNER_V2_3_1%\" \"$env.WORKSPACE/StarChef.Listener.Tests/bin/${buildMode}/StarChef.Listener.Tests.dll\" -xml \"$env.WORKSPACE/starchef-listener-xunit-results.xml\""
    	step([$class: 'XUnitPublisher', testTimeMargin: '3000', thresholdMode: 0, thresholds: [[$class: 'FailedThreshold', failureNewThreshold: '', failureThreshold: '', unstableNewThreshold: '', unstableThreshold: ''], [$class: 'SkippedThreshold', failureNewThreshold: '', failureThreshold: '', unstableNewThreshold: '', unstableThreshold: '']], tools: [[$class: 'XUnitDotNetTestType', deleteOutputFiles: true, failIfNotNew: true, pattern: '**/starchef-listener-xunit-results.xml', skipNoTestFiles: false, stopProcessingIfError: true]]])
	}

	if(isReleaseBranch()){
	 	stage("Create Package"){
			echo "Creating packages started"

 			withCredentials([string(credentialsId: 'octopus-api-key', variable: 'octopusapikey')]){
				    	def OctopusApiKey = "${octopusapikey}"
						    try {
									bat "\"${msbuild}\" \"${solutionLocation}\" /t:Build $commonReleaseBuildParams /p:Platform=\"Any CPU\" /p:RunOctoPack=\"true\" /p:OctoPackPackageVersion=\"${buildVersion}\" /p:OctoPackPublishPackageToHttp=\"https://prod-deploy.fourth.com/nuget/packages\" /p:OctoPackPublishApiKey=\"${OctopusApiKey}\""								          
								}
							finally {

							    }
			}


			
	 	}
	
	 	
	 }
}

def getVersion(fileAndPath) {
	def ver = readFile encoding: 'utf-8', file: fileAndPath
	return ver.replaceAll('\r\n', '')
}

def isReleaseBranch() {
	return env.BRANCH_NAME == "Development" || env.BRANCH_NAME == "Livemirror" || env.BRANCH_NAME == "Live" || env.BRANCH_NAME == "jenkins_pipeline"
}
