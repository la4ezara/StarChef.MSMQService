#!groovy

library 'sharedlib@master'

def onReleaseBranch() {
	return env.BRANCH_NAME == "Development" || env.BRANCH_NAME == "master" || env.BRANCH_NAME.endsWith("_HF") || env.BRANCH_NAME.endsWith("_HotFix")
}

def buildVersion = ""
def webhookUrl = "https://hooks.slack.com/services/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"

def jobUrl = "${env.RUN_DISPLAY_URL}"
def projectName = "${env.JOB_NAME}"

node {
	try{
		echo "$env.WORKSPACE"
		
		def solutionLocation = "$env.WORKSPACE/StarChef.MSMQService.sln";
		def buildMode = "Release";
		def msbuild = tool name:'MSBUILD 15', type:'msbuild'
		def sqScannerMsBuildHome = tool 'SonarQube MSBuild'
		
		def releaseTag = 'SC';
		if(env.BRANCH_NAME.endsWith("_HF") || env.BRANCH_NAME.endsWith("_HotFix")){
			releaseTag = 'HF';
		}
		
		def commonReleaseBuildParams = "/p:Configuration=Release;Platform=AnyCPU /p:CI_BuildNum=${env.BUILD_ID};CI_BuildId=${releaseTag}"
		
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
			
			withSonarQubeEnv('SonarQube Server') {
					withCredentials([string(credentialsId: 'SonarQube-github-apikey', variable: 'sonarApiKey')]) {
						def sonarPRoptions = ''
						if (!onReleaseBranch()) {
							// add options for Pull Requests
							sonarPRoptions = "/d:sonar.analysis.mode=preview /d:sonar.github.pullRequest=$env.CHANGE_ID /d:sonar.github.repository=fourth/StarChef.MSMQService /d:sonar.github.oauth=$sonarApiKey"
						}
						bat "${sqScannerMsBuildHome}/SonarQube.Scanner.MSBuild.exe begin /k:fourth:StarChefMSMQService /n:\"StarChef.MSMQService\" /v:$buildVersion" + 
							" $sonarPRoptions /d:sonar.host.url=%SONAR_HOST_URL% /d:sonar.cs.xunit.reportsPaths=\"TestResults.*.xml\" /d:sonar.cs.opencover.reportsPaths=\"opencover.xml\" /d:sonar.sourceEncoding=\"UTF-8\""
					}
				}
		}

		stage ('Build') {
			echo 'Build Start'
			bat "\"${msbuild}\" \"${solutionLocation}\" /t:Rebuild $commonReleaseBuildParams /p:Platform=\"Any CPU\""
		}

		stage('Run Unit Tests'){
			runXUnitTests("$env.WORKSPACE", "*.Tests.dll")
		}
		
		stage('SonarQube Analysis') {
				withSonarQubeEnv('SonarQube Server') {
					bat "${sqScannerMsBuildHome}\\SonarQube.Scanner.MSBuild.exe end"
				}
			}
			
		currentBuild.description = 'Binaries built and tested'

		if(onReleaseBranch()){
			stage("Create Package"){
				echo "Creating packages started"
				
				def releaseNotesMdFile = "$env.WORKSPACE/Auto-ReleaseNotes.md"
					generateMarkdownReleaseNotes(buildVersion, releaseNotesMdFile)

				withCredentials([string(credentialsId: 'octopus-api-key', variable: 'octopusapikey')]){
					def OctopusApiKey = "${octopusapikey}"
					try {
							bat "\"${msbuild}\" \"${solutionLocation}\" /t:Build $commonReleaseBuildParams /p:Platform=\"Any CPU\" /p:RunOctoPack=\"true\" /p:OctoPackPackageVersion=\"${buildVersion}\" /p:OctoPackPublishPackageToHttp=\"https://prod-deploy.fourth.com/nuget/packages\" /p:OctoPackPublishApiKey=\"${OctopusApiKey}\""								          
					}
					finally {

					}
				}				
			}
		
			currentBuild.displayName = '# ' + buildVersion
			currentBuild.description = "Nuget packages for version ${buildVersion} created"
			
			stage('Send Notifications') {
				def changes = getChangeString('- ', '\\n').replaceAll(/"/, /\\"/).replaceAll(/'/, /\\'/)
				def msgCard = getSlackSuccessCard(projectName, jobUrl, buildVersion, changes)
				sendSlackNotification(webhookUrl, msgCard)
			}
	 	
		}
		
		deleteDir()
	 }catch (ex) {
		def errorText = "$ex"
		if (onReleaseBranch()) {
			// notify in case of error
			echo "Error in build job on release branch. Details: $errorText"
			def msgCard = getSlackFailureCard(projectName, jobUrl, errorText)
			sendSlackNotification(webhookUrl, msgCard)
		}
		throw ex
	}
}

def getVersion(fileAndPath) {
	def ver = readFile encoding: 'utf-8', file: fileAndPath
	return ver.replaceAll('\r\n', '')
}

def getPassedBuilds() {
	def passedBuilds = []

	def build = currentBuild
	while ((build != null) && (build.result != 'SUCCESS'))  {
		passedBuilds.add(build)
		build = build.previousBuild
	}
	return passedBuilds
}

def getBuildChangeString(build, itemBegin, itemEnd) {
	MAX_MSG_LEN = 192

	def changeString = ""

	def changeLogSets = build.changeSets
	for (int i = 0; i < changeLogSets.size(); i++) {
		def entries = changeLogSets[i].items
		for (int j = 0; j < entries.length; j++) {
			def entry = entries[j]
			truncated_msg = entry.msg.take(MAX_MSG_LEN).replaceAll('\r', ' ').replaceAll('\n', ' ')
			changeString += "${itemBegin}${entry.author}: ${entry.msg}${itemEnd}"
		}
	}
	return changeString
}

def getChangeString(itemBegin, itemEnd) {
	def accumulatedChanges = ""
	
	def passedBuilds = getPassedBuilds()
	for (int x = 0; x < passedBuilds.size(); x++) {
		accumulatedChanges += getBuildChangeString(passedBuilds[x], itemBegin, itemEnd)
	}

	if (!accumulatedChanges) {
		accumulatedChanges = "${itemBegin}No changes${itemEnd}"
	}
	return accumulatedChanges
}

def generateChangelogInNuspecFile (fileAndPath)
{
	def nuspec = readFile encoding: 'utf-8', file: fileAndPath
	def changeStringData = '<![CDATA[' + getChangeString('- ', '\r\n') + ']]>'
	def str2 = nuspec.replaceAll('__changelog__', changeStringData)
	
	writeFile encoding: 'utf-8', file: fileAndPath, text: str2
}

def generateMarkdownReleaseNotes (buildVersion, fileAndPath) 
{
	def changes = getChangeString('* ', '\n')
	def fileContents = """
##StarChef.MSMQService v.$buildVersion
Changes included
$changes
	"""
	writeFile encoding: 'utf-8', file: fileAndPath, text: fileContents
}
